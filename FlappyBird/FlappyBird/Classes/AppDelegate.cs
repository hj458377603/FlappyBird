using cocos2d;
using FlappyBird.Classes.Scenes;
using Microsoft.Xna.Framework;

namespace WindowsPhoneGame2.Classes
{
    public class AppDelegate : CCApplication
    {
        private Game game;

        public static CCSize screenSize;

        public AppDelegate(Game game, GraphicsDeviceManager graphics)
            : base(game, graphics)
        {
            this.game = game;
            CCApplication.sm_pSharedApplication = this;
            this.setOrientation(Orientation.kOrientationLandscapeLeft);
        }

        /// <summary>
        /// Implement for initialize OpenGL instance, set source path, etc...
        /// </summary>
        public override bool initInstance()
        {
            return base.initInstance();
        }

        /// <summary>
        ///  Implement CCDirector and CCScene init code here.
        /// </summary>
        /// <returns>
        ///  true  Initialize success, app continue.
        ///  false Initialize failed, app terminate.
        /// </returns>
        public override bool applicationDidFinishLaunching()
        {
            //initialize director
            CCDirector pDirector = CCDirector.sharedDirector();
            pDirector.setOpenGLView();

            //turn on display FPS
            pDirector.DisplayFPS = true;

            // pDirector->setDeviceOrientation(kCCDeviceOrientationLandscapeLeft);

            // set FPS. the default value is 1.0/60 if you don't call this
            pDirector.animationInterval = 1.0 / 60;

            screenSize = CCDirector.sharedDirector().getWinSize();

            // create a scene. it's an autorelease object
            CCScene loadScene = new StartScene(game);

            //run
            pDirector.runWithScene(loadScene);
            return true;
        }

        /// <summary>
        /// The function be called when the application enter background
        /// </summary>
        public override void applicationDidEnterBackground()
        {
            CCDirector.sharedDirector().pause();

            // if you use SimpleAudioEngine, it must be pause
            // SimpleAudioEngine::sharedEngine()->pauseBackgroundMusic();
        }

        /// <summary>
        /// The function be called when the application enter foreground  
        /// </summary>
        public override void applicationWillEnterForeground()
        {
            CCDirector.sharedDirector().resume();

            // if you use SimpleAudioEngine, it must resume here
            // SimpleAudioEngine::sharedEngine()->resumeBackgroundMusic();
        }
    }
}
