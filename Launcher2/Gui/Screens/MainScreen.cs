﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using System.IO;
using System.Net;
using ClassicalSharp;
using Launcher.Gui.Views;
using Launcher.Web;
using Launcher.Gui.Widgets;

namespace Launcher.Gui.Screens {
	
	public sealed partial class MainScreen : LauncherInputScreen {
		
		MainView view;
		public MainScreen( LauncherWindow game ) : base( game, true ) {
			enterIndex = 2;
			view = new MainView( game );
			widgets = view.widgets;
			LoadResumeInfo();
		}
		
		public override void Init() {
			base.Init();
			view.Init();
			SetupWidgetHandlers();
			Resize();
			
			using( drawer ) {
				drawer.SetBitmap( game.Framebuffer );
				LoadSavedInfo();
			}
		}
		
		public override void Resize() {
			view.DrawAll();
			game.Dirty = true;
		}
		
		bool updateDone;
		void SuccessfulUpdateCheck( UpdateCheckTask task ) {
			string latestVer = game.checkTask.LatestStable.Version.Substring( 1 );
			int spaceIndex = Program.AppName.LastIndexOf( ' ' );
			string currentVer = Program.AppName.Substring( spaceIndex + 1 );
			bool update = new Version( latestVer ) > new Version( currentVer );
			
			view.updateText = update ? "&aNew release available" : "&eUp to date     ";
			game.RedrawBackground();
			Resize();
			SelectWidget( selectedWidget );
		}
		
		void FailedUpdateCheck( UpdateCheckTask task ) {
			view.updateText = "&cUpdate check failed    ";
			game.RedrawBackground();
			Resize();
			SelectWidget( selectedWidget );
		}
		
		void SetupWidgetHandlers() {
			widgets[view.loginIndex].OnClick = LoginAsync;
			widgets[view.resIndex].OnClick = ResumeClick;
			widgets[view.dcIndex].OnClick =
				(x, y) => game.SetScreen( new DirectConnectScreen( game ) );
			widgets[view.spIndex].OnClick =
				(x, y) => Client.Start( widgets[0].Text, ref game.ShouldExit );
			
			if( widgets[view.colIndex] != null )
				widgets[view.colIndex].OnClick = 
					(x, y) => game.SetScreen( new ColoursScreen( game ) );
			
			widgets[view.updatesIndex].OnClick =
				(x, y) => game.SetScreen( new UpdatesScreen( game ) );
			widgets[view.modeIndex].OnClick =
				(x, y) => game.SetScreen( new ChooseModeScreen( game, false ) );
			SetupInputHandlers();
		}

		const int buttonWidth = 220, buttonHeight = 35, sideButtonWidth = 150;
		string resumeUser, resumeIp, resumePort, resumeMppass;
		bool resumeCCSkins, resumeValid;
		
		void LoadResumeInfo() {
			resumeUser = Options.Get( "launcher-username" );
			resumeIp = Options.Get( "launcher-ip" ) ?? "";
			resumePort = Options.Get( "launcher-port" ) ?? "";
			resumeCCSkins = Options.GetBool( "launcher-ccskins", true );
			
			IPAddress address;
			if( !IPAddress.TryParse( resumeIp, out address ) ) resumeIp = "";
			ushort portNum;
			if( !UInt16.TryParse( resumePort, out portNum ) ) resumePort = "";
			
			string mppass = Options.Get( "launcher-mppass" ) ?? null;
			resumeMppass = Secure.Decode( mppass, resumeUser );
			resumeValid = !String.IsNullOrEmpty( resumeUser ) && !String.IsNullOrEmpty( resumeIp )
				&& !String.IsNullOrEmpty( resumePort ) && !String.IsNullOrEmpty( resumeMppass );
		}
		
		void ResumeClick( int mouseX, int mouseY ) {
			if( !resumeValid ) return;
			ClientStartData data = new ClientStartData( resumeUser, resumeMppass, resumeIp, resumePort );
			Client.Start( data, resumeCCSkins, ref game.ShouldExit );
		}
		
		protected override void SelectWidget( LauncherWidget widget ) {
			base.SelectWidget( widget );
			if( signingIn || !resumeValid || widget != widgets[4] ) return;
			const string format = "&eResume to {0}:{1}, as {2}";
			SetStatus( String.Format( format, resumeIp, resumePort, resumeUser ) );
		}
		
		protected override void UnselectWidget( LauncherWidget widget ) {
			base.UnselectWidget( widget );
			if( signingIn || !resumeValid || widget != widgets[4] ) return;
			SetStatus( "" );
		}
		
		public override void Dispose() {
			StoreFields();
			base.Dispose();
		}
	}
}
