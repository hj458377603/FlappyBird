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
