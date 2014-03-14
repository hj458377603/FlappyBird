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
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");
            bird.position = new CCPoint(screenSize.width / 2, screenSize.height / 2);
            this.addChild(bird);
        }

        #endregion
    }
}
