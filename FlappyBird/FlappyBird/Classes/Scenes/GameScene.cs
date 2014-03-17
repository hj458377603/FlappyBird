using Box2D.Collision.Shapes;
using Box2D.Common;
using Box2D.Dynamics;
using cocos2d;
using FlappyBird.Classes.ContactListeners;
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

        // 物理世界中游戏主角bird
        private b2Body birdBody;

        // 障碍物层
        private CCLayer barLayer;

        // 得分文本
        private CCLabelTTF scoreLabel;

        // 得分值
        private int score = 0;

        // 游戏主角bird是否还活着
        private bool isBirdAlive = true;
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
            InitBack();
            AddBarLayer();
            AddBar(0.5f);
            AddGround(0);
            AddBird();
            world.SetContactListener(new BirdContactListener(this, (CCSprite)(birdBody.UserData)));
            this.schedule(tick);

            InitScore();
        }

        #endregion

        #region 重写方法

        #endregion

        #region 方法

        /// <summary>
        /// 初始化背景图片
        /// </summary>
        private void InitBack()
        {
            CCSprite back = CCSprite.spriteWithFile("imgs/back/back");
            back.anchorPoint = new CCPoint(0, 1);
            back.position = new CCPoint(0, screenSize.height);
            this.addChild(back);
        }

        /// <summary>
        /// 第一步在场景中添加一个静态的bird精灵
        /// 第二步让bird做飞行动作
        /// 第三步利用box-2d物理引擎让bird在物理世界中飞行
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");
            bird.rotation = -15;
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
        private void AddGround(float interval)
        {
            CCSprite ground = CCSprite.spriteWithFile("imgs/ground/ground_01");

            b2BodyDef groundBodyDef = new b2BodyDef();
            groundBodyDef.position = new b2Vec2(ground.contentSize.width / PTM_RATIO / 2, ground.contentSize.height / PTM_RATIO / 2);
            groundBodyDef.userData = ground;
            groundBodyDef.type = b2BodyType.b2_kinematicBody;

            b2Body groundBody = world.CreateBody(groundBodyDef);
            b2PolygonShape groundBox = new b2PolygonShape();
            b2FixtureDef boxShapeDef = new b2FixtureDef();
            boxShapeDef.shape = groundBox;
            groundBox.SetAsBox(ground.contentSize.width / PTM_RATIO / 2, ground.contentSize.height / PTM_RATIO / 2);

            groundBody.CreateFixture(boxShapeDef);
            this.addChild(ground);
        }

        /// <summary>
        /// 添加遮挡障碍
        /// </summary>
        /// <param name="interval"></param>
        private void AddBar(float interval)
        {
            int offset = new Random().Next(-160, 50);

            // upBar
            CCSprite upBar = CCSprite.spriteWithFile("imgs/bar/up_bar");
            b2BodyDef upBarBodyDef = new b2BodyDef();
            upBarBodyDef.position = new b2Vec2(screenSize.width / PTM_RATIO, (screenSize.height + offset + 80) / PTM_RATIO);
            upBarBodyDef.userData = upBar;
            upBarBodyDef.type = b2BodyType.b2_kinematicBody;

            b2Body upBarBody = world.CreateBody(upBarBodyDef);

            b2PolygonShape upBarBox = new b2PolygonShape();
            b2FixtureDef boxShapeDef = new b2FixtureDef();
            boxShapeDef.shape = upBarBox;
            upBarBox.SetAsBox(upBar.contentSize.width / PTM_RATIO / 2, upBar.contentSize.height / PTM_RATIO / 2);

            upBarBody.LinearVelocity = new b2Vec2(-3, 0);
            upBarBody.CreateFixture(boxShapeDef);

            barLayer.addChild(upBar);

            // downBar
            CCSprite downBar = CCSprite.spriteWithFile("imgs/bar/down_bar");
            b2BodyDef downBarBodyDef = new b2BodyDef();
            downBarBodyDef.position = new b2Vec2(screenSize.width / PTM_RATIO, (downBar.contentSize.height + offset * 2 - 80) / 2 / PTM_RATIO);
            downBarBodyDef.userData = downBar;
            downBarBodyDef.type = b2BodyType.b2_kinematicBody;

            b2Body downBarBody = world.CreateBody(downBarBodyDef);

            b2PolygonShape downBarBox = new b2PolygonShape();
            b2FixtureDef shapeDef = new b2FixtureDef();
            shapeDef.shape = downBarBox;
            downBarBox.SetAsBox(downBar.contentSize.width / PTM_RATIO / 2, downBar.contentSize.height / PTM_RATIO / 2);

            downBarBody.LinearVelocity = new b2Vec2(-3, 0);
            downBarBody.CreateFixture(shapeDef);
            barLayer.addChild(downBar);

            if (scoreLabel != null)
            {
                scoreLabel.setString(score++.ToString());
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        private void AddBarLayer()
        {
            barLayer = new CCLayer();
            this.addChild(barLayer);

            CCScheduler.sharedScheduler().scheduleSelector(AddBar, this, 2f, true);
        }

        /// <summary>
        /// 初始化得分
        /// </summary>
        private void InitScore()
        {
            scoreLabel = CCLabelTTF.labelWithString(score.ToString(), "Arial", 20);
            scoreLabel.Color = new ccColor3B(Color.Red);
            scoreLabel.position = new CCPoint(screenSize.width - 20, screenSize.height - 20);
            CCLayer scoreLayer = new CCLayer();
            scoreLayer.addChild(scoreLabel);
            this.addChild(scoreLayer);
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
                    CCSprite sprite = (CCSprite)b.UserData;
                    sprite.position = new CCPoint((float)(b.Position.x * PTM_RATIO),
                                                    (float)(b.Position.y * PTM_RATIO));

                    //sprite.rotation = -1 * MathHelper.ToDegrees(b.Angle);

                    if (birdBody.UserData == sprite && sprite.rotation < 90)
                    {
                        if (sprite.rotation < 0)
                        {
                            sprite.rotation += 1;
                        }
                        else
                        {
                            sprite.rotation += 5;
                        }
                    }

                    // 销毁不在屏幕中的对象
                    if (b.Position.x < -3)
                    {
                        sprite.removeFromParentAndCleanup(true);
                        world.DestroyBody(b);
                    }

                }
            }
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
            if (isBirdAlive)
            {
                birdBody.LinearVelocity = new b2Vec2(0, 10);
                ((CCSprite)(birdBody.UserData)).rotation = -30;
            }
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

        /// <summary>
        /// 游戏结束
        /// </summary>
        public void GameOver()
        {
            isBirdAlive = false;
            var bird = (CCSprite)birdBody.UserData;
            CCMoveTo moveTo = CCMoveTo.actionWithDuration(0.5f, new CCPoint(bird.position.x, 120));
            CCRotateTo rotateTo = CCRotateTo.actionWithDuration(0.1f, 90);
            bird.runAction(moveTo);
            bird.runAction(rotateTo);
            bird.stopActionByTag(0);
            CCScheduler.sharedScheduler().unscheduleSelector(AddBar, this);
            this.unschedule(tick);
        }
        #endregion
    }
}
