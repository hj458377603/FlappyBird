using cocos2d;
using CocosDenshion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhoneGame2.Classes;

namespace FlappyBird.Classes.Scenes
{
    class LoadingScene : CCScene, ICCTargetedTouchDelegate
    {
        // 加载文本
        private CCLabelTTF loadLabel;

        // 加载是否完成
        private bool isLoaded = false;

        public LoadingScene()
        {
            CCCallFunc initFunc = CCCallFunc.actionWithTarget(this, Init);
            CCCallFunc loadFunc = CCCallFunc.actionWithTarget(this, LoadMusics);

            var tempAction = CCSequence.actions(initFunc, CCDelayTime.actionWithDuration(1f), loadFunc);
            this.runAction(tempAction);
        }

        private void Init()
        {
            loadLabel = CCLabelTTF.labelWithString("Loading...", "Arial", 20);
            loadLabel.position = new CCPoint(AppDelegate.screenSize.width / 2, AppDelegate.screenSize.height / 2);
            this.addChild(loadLabel);
        }

        /// <summary>
        /// 加载音乐文件
        /// </summary>
        private void LoadMusics()
        {
            SimpleAudioEngine.sharedEngine().preloadBackgroundMusic(@"musics/background");

            SimpleAudioEngine.sharedEngine().preloadEffect(@"musics/sfx_wing");
            SimpleAudioEngine.sharedEngine().preloadEffect(@"musics/sfx_hit");
            SimpleAudioEngine.sharedEngine().preloadEffect(@"musics/sfx_die");
            SimpleAudioEngine.sharedEngine().preloadEffect(@"musics/sfx_point");

            #region label缩放效果

            CCScaleTo scale1 = CCScaleTo.actionWithDuration(0.5f, 0.9f);

            CCScaleTo scale2 = CCScaleTo.actionWithDuration(0.5f, 1.1f);
            var actions = CCSequence.actions(scale1, scale2);

            var action = CCRepeat.actionWithAction(actions, 20);
            loadLabel.runAction(action);

            loadLabel.setString("Tap");

            #endregion

            isLoaded = true;
        }

        public override void onEnter()
        {
            // 注册点击事件
            CCTouchDispatcher.sharedDispatcher().addTargetedDelegate(this, 0, true);
            base.onEnter();
        }

        public override void onExit()
        {
            // 移除点击事件
            CCTouchDispatcher.sharedDispatcher().removeDelegate(this);
            base.onExit();
        }

        public bool ccTouchBegan(CCTouch pTouch, CCEvent pEvent)
        {
            if (isLoaded)
            {
                // 移除loadLabel
                if (loadLabel != null && loadLabel.parent != null)
                {
                    loadLabel.removeFromParentAndCleanup(true);
                    CCDirector.sharedDirector().replaceScene(new StartScene());
                }
                return true;
            }
            return false;
        }

        public void ccTouchCancelled(CCTouch pTouch, CCEvent pEvent)
        {

        }

        public void ccTouchEnded(CCTouch pTouch, CCEvent pEvent)
        {

        }

        public void ccTouchMoved(CCTouch pTouch, CCEvent pEvent)
        {

        }
    }
}
