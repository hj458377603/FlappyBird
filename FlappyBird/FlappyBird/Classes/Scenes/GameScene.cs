using cocos2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlappyBird.Classes.Scenes
{
    class GameScene : CCScene
    {
        #region �ֶ�
        private CCSize screenSize = CCDirector.sharedDirector().getWinSize();
        #endregion

        #region ����

        #endregion

        #region ���췽��

        public GameScene()
        {
            AddBird();
        }

        #endregion

        #region ��д����

        #endregion

        #region ����

        /// <summary>
        /// ��һ���ڳ��������һ����̬��bird����
        /// �ڶ�����bird�����ж���
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");
            bird.position = new CCPoint(screenSize.width / 2, screenSize.height / 2);

            // bird���ж���֡����
            List<CCSpriteFrame> frames = new List<CCSpriteFrame>();

            for (int i = 1; i < 3; i++)
            {
                // ֡��ͼ
                CCTexture2D texture = CCTextureCache.sharedTextureCache().addImage("imgs/bird/bird_0" + i);

                // �������һ�������bug����������õĻ����ͻᲥ�Ų���������
                texture.Name = (uint)i;
                var frame = CCSpriteFrame.frameWithTexture(texture, new CCRect(0, 0, texture.ContentSizeInPixels.width, texture.ContentSizeInPixels.height));
                frames.Add(frame);
            }

            // ���ж���
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
