using cocos2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhoneGame2.Classes;

namespace FlappyBird.Classes.Scenes
{
    class StartScene : CCScene
    {
        public StartScene()
        {
            InitBackGround();
        }

        private void InitBackGround()
        {
            #region 地面
            CCSprite ground = CCSprite.spriteWithFile("imgs/ground/ground_01");
            ground.position = new CCPoint(AppDelegate.screenSize.width / 2, ground.contentSize.height / 2);
            this.addChild(ground);
            #endregion

            #region 天空
            CCSprite sky = CCSprite.spriteWithFile("imgs/back/back");
            sky.anchorPoint = new CCPoint(0, 1);
            sky.position = new CCPoint(0, AppDelegate.screenSize.height);
            this.addChild(sky);
            #endregion

            #region FlappyBird图片文本
            CCSprite flappyBirdTxt = CCSprite.spriteWithFile("imgs/start/fb_label");
            flappyBirdTxt.position = new CCPoint(AppDelegate.screenSize.width / 2, (2 / 3f) * AppDelegate.screenSize.height);
            this.addChild(flappyBirdTxt);
            #endregion

            #region FlappyBird

            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");
            bird.position = new CCPoint(AppDelegate.screenSize.width / 2, AppDelegate.screenSize.height / 2);
            // bird飞行动作帧集合
            List<CCSpriteFrame> frames = new List<CCSpriteFrame>();

            for (int i = 1; i < 3; i++)
            {
                // 帧贴图
                CCTexture2D texture = CCTextureCache.sharedTextureCache().addImage("imgs/bird/bird_0" + i);

                // 这里存在一个引擎的bug，如果不设置的话，就会播放不出来动画
                texture.Name = (uint)i;
                var frame = CCSpriteFrame.frameWithTexture(texture, new CCRect(0, 0, texture.ContentSizeInPixels.width, texture.ContentSizeInPixels.height));
                frames.Add(frame);
            }

            // 飞行动画
            CCAnimation marmotShowanimation = CCAnimation.animationWithFrames(frames, 0.1f);
            CCAnimate flyAction = CCAnimate.actionWithAnimation(marmotShowanimation, false);
            CCRepeatForever repeatAction = CCRepeatForever.actionWithAction(flyAction);
            bird.runAction(repeatAction);
            this.addChild(bird);
            #endregion

            #region 按钮
            // 开始按钮
            CCSprite btnStart = CCSprite.spriteWithFile("imgs/start/btn_start");
            CCMenuItemSprite menuItemSprite1 = CCMenuItemSprite.itemFromNormalSprite(btnStart, null, this, (sender) =>
            {
                GameScene gameScene = new GameScene();

                // 跳转到下一个场景
                //var scene = CCTransitionMoveInR.transitionWithDuration(0.1f, gameScene);
                var scene = CCTransitionFade.transitionWithDuration(0.5f, gameScene);
                CCDirector.sharedDirector().pushScene(scene);
            });

            // 排行榜
            CCSprite btnHighScore = CCSprite.spriteWithFile("imgs/start/btn_highScore");
            CCMenuItemSprite menuItemSprite2 = CCMenuItemSprite.itemFromNormalSprite(btnHighScore, null, this, (sender) =>
            {

            });

            CCMenu menu = CCMenu.menuWithItems(menuItemSprite1, menuItemSprite2);
            menu.position = new CCPoint(AppDelegate.screenSize.width / 2, ground.contentSize.height + btnStart.contentSize.height / 2);

            //菜单水平排列
            menu.alignItemsHorizontallyWithPadding(48);
            this.addChild(menu);
            #endregion
        }
    }
}
