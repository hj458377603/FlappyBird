using cocos2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlappyBird.Classes.Scenes
{
    class GameScene : CCScene
    {
        #region 字段
        private CCSize screenSize = CCDirector.sharedDirector().getWinSize();
        #endregion

        #region 属性

        #endregion

        #region 构造方法

        public GameScene()
        {
            AddBird();
        }

        #endregion

        #region 重写方法

        #endregion

        #region 方法

        /// <summary>
        /// 第一步在场景中添加一个静态的bird精灵
        /// 第二步让bird做飞行动作
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");
            bird.position = new CCPoint(screenSize.width / 2, screenSize.height / 2);

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
            repeatAction.tag = 0;
            bird.runAction(repeatAction);

            this.addChild(bird);
        }

        #endregion
    }
}
