﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Collections.Generic;
using System.Net;
using ClassicalSharp;
using Launcher.Gui.Widgets;
using Launcher.Web;

namespace Launcher.Gui.Screens {
	
	public sealed partial class MainScreen : LauncherInputScreen {
		
		public override void Tick() {
			base.Tick();
			if( game.checkTask != null && game.checkTask.Done && !updateDone ) {
				bool success = game.checkTask.Success;
				if( success ) SuccessfulUpdateCheck( game.checkTask );
				else FailedUpdateCheck( game.checkTask );
				updateDone = true;
			}
			
			if( !signingIn ) return;		
			ClassicubeSession session = game.Session;
			string status = session.Status;
			if( status != lastStatus )
				SetStatus( status );
			
			if( session.Working ) return;			
			if( session.Exception != null ) {
				DisplayWebException( session.Exception, session.Status );
			} else if( HasServers ) {
				game.SetScreen( new ServersScreen( game ) );
				return;
			}
			
			signingIn = false;
			game.RedrawBackground();
			Resize();
		}

		string lastStatus;
		void SetStatus( string text ) {
			lastStatus = text;
			LauncherLabelWidget widget = (LauncherLabelWidget)widgets[3];
			
			game.ClearArea( widget.X, widget.Y, widget.Width, widget.Height );
			widget.SetDrawData( drawer, text, inputFont, Anchor.Centre, Anchor.Centre, 0, 20 );
			RedrawWidget( widget );
			game.Dirty = true;
		}
		
		bool HasServers {
			get { return game.Session.Servers != null && game.Session.Servers.Count != 0; }
		}
		
		bool signingIn;
		void LoginAsync( int mouseX, int mouseY ) {
			if( String.IsNullOrEmpty( Get( 0 ) ) ) {
				SetStatus( "&ePlease enter a username" ); return;
			}
			if( String.IsNullOrEmpty( Get( 1 ) ) ) {
				SetStatus( "&ePlease enter a password" ); return;
			}
			if( signingIn ) return;
			UpdateSignInInfo( Get( 0 ), Get( 1 ) );
			
			LauncherBoolWidget skip = widgets[view.sslIndex] as LauncherBoolWidget;
			if( skip != null && skip.Value ) {
				ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
				Options.Set( "skip-ssl-check", true );
			} else {
				ServicePointManager.ServerCertificateValidationCallback = null;
			}
			
			game.Session.LoginAsync( Get( 0 ), Get( 1 ) );
			game.RedrawBackground();
			Resize();
			SetStatus( "&eSigning in.." );
			signingIn = true;
		}

		void DisplayWebException( WebException ex, string action ) {
			ErrorHandler.LogError( action, ex );
			bool sslCertError = ex.Status == WebExceptionStatus.TrustFailure ||
				(ex.Status == WebExceptionStatus.SendFailure && OpenTK.Configuration.RunningOnMono);
			if( ex.Status == WebExceptionStatus.Timeout ) {
				string text = "&eFailed to " + action + ":" +
					Environment.NewLine + "Timed out while connecting to classicube.net.";
				SetStatus( text );
			} else if( ex.Status == WebExceptionStatus.ProtocolError ) {
				HttpWebResponse response = (HttpWebResponse)ex.Response;
				int errorCode = (int)response.StatusCode;
				string description = response.StatusDescription;
				string text = "&eFailed to " + action + ":" +
					Environment.NewLine + " classicube.net returned: (" + errorCode + ") " + description;
				SetStatus( text );
			} else if( ex.Status == WebExceptionStatus.NameResolutionFailure ) {
				string text = "&eFailed to " + action + ":" +
					Environment.NewLine + "Unable to resolve classicube.net" +
					Environment.NewLine + "you may not be connected to the internet.";
				SetStatus( text );
			} else if( sslCertError ) {
				string text = "&eFailed to validate SSL certificate";
				SetStatus( text );
				using( drawer ) {
					drawer.SetBitmap( game.Framebuffer );
					view.widgetIndex = view.sslIndex;
					view.MakeSslWidgets();
					
					widgets[view.sslIndex].OnClick = SSLSkipValidationClick;
					widgets[view.sslIndex].Redraw( drawer );
					widgets[view.sslIndex + 1].Redraw( drawer );
				}
			} else {
				string text = "&eFailed to " + action + ":" +
					Environment.NewLine + ex.Status;
				SetStatus( text );
			}
		}
		
		void SSLSkipValidationClick( int mouseX, int mouseY ) {
			using( drawer ) {
				drawer.SetBitmap( game.Framebuffer );
				LauncherBoolWidget widget = (LauncherBoolWidget)widgets[view.sslIndex];
				SetBool( !widget.Value );
			}
		}
		
		void SetBool( bool value ) {
			LauncherBoolWidget widget = (LauncherBoolWidget)widgets[view.sslIndex];
			widget.Value = value;
			widget.Redraw( game.Drawer );
			game.Dirty = true;
		}
		
		void StoreFields() {
			Dictionary<string, object> metadata;
			if( !game.ScreenMetadata.TryGetValue( "screen-CC", out metadata ) ) {
				metadata = new Dictionary<string, object>();
				game.ScreenMetadata["screen-CC"] = metadata;
			}
			metadata["user"] = Get( 0 );
			metadata["pass"] = Get( 1 );
		}
		
		void LoadSavedInfo() {
			Dictionary<string, object> metadata;
			// restore what user last typed into the various fields
			if( game.ScreenMetadata.TryGetValue( "screen-CC", out metadata ) ) {
				Set( 0, (string)metadata["user"] );
				Set( 1, (string)metadata["pass"] );
			} else {
				LoadFromOptions();
			}
		}
		
		void LoadFromOptions() {
			if( !Options.Load() )
				return;
			
			string user = Options.Get( "launcher-cc-username" ) ?? "";
			string pass = Options.Get( "launcher-cc-password" ) ?? "";
			pass = Secure.Decode( pass, user );
			
			Set( 0, user );
			Set( 1, pass );
		}
		
		void UpdateSignInInfo( string user, string password ) {
			// If the client has changed some settings in the meantime, make sure we keep the changes
			if( !Options.Load() )
				return;
			
			Options.Set( "launcher-cc-username", user );
			Options.Set( "launcher-cc-password", Secure.Encode( password, user ) );
			Options.Save();
		}
	}
}
