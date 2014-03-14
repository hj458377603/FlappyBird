using Box2D.Collision.Shapes;
using Box2D.Common;
using Box2D.Dynamics;
using cocos2d;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlappyBird.Classes.Scenes
{
    class GameScene : CCScene, ICCTargetedTouchDelegate
    {
        #region 字段

        // 屏幕的宽高
        private CCSize screenSize = CCDirector.sharedDirector().getWinSize();

        // 屏幕与物理世界的大小的映射比例 
        // 具体可参考：http://blog.csdn.net/zhangxaochen/article/details/8009508
        private float PTM_RATIO = 32.0f;

        // 物理世界
        private b2World world = null;

        // 游戏主角bird
        private b2Body birdBody;
        #endregion

        #region 属性

        #endregion

        #region 构造方法

        public GameScene()
        {
            // 初始化world
            b2Vec2 gravity = new b2Vec2(0f, -20f);
            world = new b2World(gravity);
            world.AllowSleep = false;

            AddGround();
            AddBird();

            this.schedule(tick);
        }

        #endregion

        #region 重写方法

        #endregion

        #region 方法

        /// <summary>
        /// 第一步在场景中添加一个静态的bird精灵
        /// 第二步让bird做飞行动作
        /// 第三步利用box-2d物理引擎让bird在物理世界中飞行
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");

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

            // 在物理世界中定义一个body，设置其位置，并让bird与之对应
            b2BodyDef ballBodyDef = new b2BodyDef();
            ballBodyDef.type = b2BodyType.b2_dynamicBody;
            ballBodyDef.position = new b2Vec2(screenSize.width / PTM_RATIO / 2, (float)(screenSize.height / PTM_RATIO));
            ballBodyDef.userData = bird;
            birdBody = world.CreateBody(ballBodyDef);

            // 为body创建形状，并设置一些物理属性
            b2PolygonShape shape = new b2PolygonShape();
            shape.SetAsBox(bird.contentSize.width / 2 / PTM_RATIO, bird.contentSize.height / 2 / PTM_RATIO);
            b2FixtureDef fixtureDef = new b2FixtureDef();
            fixtureDef.shape = shape;
            fixtureDef.density = 500.0f;
            fixtureDef.friction = 0.5f;
            fixtureDef.restitution = 0.1f;
            birdBody.CreateFixture(fixtureDef);

            this.addChild(bird);
        }

        /// <summary>
        /// 第一步添加地面
        /// 第二步利用box-2d物理引擎将地面作为静态物体添加到物理世界中
        /// </summary>
        private void AddGround()
        {
            CCSprite ground = CCSprite.spriteWithFile("imgs/ground/ground");
            b2BodyDef groundBodyDef = new b2BodyDef();
            groundBodyDef.position = new b2Vec2(ground.contentSize.width / PTM_RATIO / 2, ground.contentSize.height / PTM_RATIO / 2);
            groundBodyDef.userData = ground;
            groundBodyDef.type = b2BodyType.b2_staticBody;

            b2Body groundBody = world.CreateBody(groundBodyDef);

            b2PolygonShape groundBox = new b2PolygonShape();
            b2FixtureDef boxShapeDef = new b2FixtureDef();
            boxShapeDef.shape = groundBox;
            groundBox.SetAsBox(ground.contentSize.width / PTM_RATIO / 2, ground.contentSize.height / PTM_RATIO / 2);

            groundBody.CreateFixture(boxShapeDef);
            this.addChild(ground);
        }

        /// <summary>
        /// 模拟物理世界
        /// </summary>
        /// <param name="dt"></param>
        private void tick(float dt)
        {
            world.Step(dt, 10, 10);
            for (b2Body b = world.BodyList; b != null; b = b.Next)
            {
                if (b.UserData != null)
                {
                    CCSprite ballData = (CCSprite)b.UserData;

                    ballData.position = new CCPoint((float)(b.Position.x * PTM_RATIO),
                                                    (float)(b.Position.y * PTM_RATIO));

                    ballData.rotation = -1 * MathHelper.ToDegrees(b.Angle);
                }
            }
        }

        public override void onEnter()
        {
            CCTouchDispatcher.sharedDispatcher().addTargetedDelegate(this, 0, true);
            base.onEnter();
        }

        public override void onExit()
        {
            CCTouchDispatcher.sharedDispatcher().removeDelegate(this);
            base.onExit();
        }

        public bool ccTouchBegan(CCTouch pTouch, CCEvent pEvent)
        {
            birdBody.LinearVelocity = new b2Vec2(birdBody.LinearVelocity.x, 10);
            return true;
        }

        public void ccTouchCancelled(CCTouch pTouch, CCEvent pEvent)
        {
            //throw new NotImplementedException();
        }

        public void ccTouchEnded(CCTouch pTouch, CCEvent pEvent)
        {
            //throw new NotImplementedException();
        }

        public void ccTouchMoved(CCTouch pTouch, CCEvent pEvent)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
