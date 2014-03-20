using Box2D.Collision.Shapes;
using Box2D.Common;
using Box2D.Dynamics;
using cocos2d;
using CocosDenshion;
using FlappyBird.Classes.ContactListeners;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using WindowsPhoneGame2.Classes;

namespace FlappyBird.Classes.Scenes
{
    class GameScene : CCScene, ICCTargetedTouchDelegate
    {
        #region 字段
        private Game game;

        // 屏幕与物理世界的大小的映射比例 
        // 具体可参考：http://blog.csdn.net/zhangxaochen/article/details/8009508
        private float PTM_RATIO = 32.0f;

        // 物理世界
        private b2World world = null;

        // 物理世界中游戏主角bird
        private b2Body birdBody;

        // 障碍物层
        private CCLayer barLayer;

        // 得分层
        private CCLayer scoreLayer;

        // 得分值
        private int score = 0;

        // 游戏主角bird是否还活着
        private bool isBirdAlive = true;

        // 得分值
        private List<CCSprite> digitSprites;

        // 飞行速度
        private float flySpeed = 3;

        // 障碍物
        private Queue<CCSprite> upBars;

        // 障碍物产生时间间隔
        private readonly float createBarInterval = 1.5f;

        // 地面
        private CCSprite ground;

        // 游戏结束弹出的Layer
        private CCLayer gameOverLayer;

        private int highestScore;

        #endregion

        #region 属性

        #endregion

        #region 构造方法

        public GameScene(Game game)
        {
            this.game = game;
            // 初始化world
            b2Vec2 gravity = new b2Vec2(0f, -20f);
            world = new b2World(gravity);
            world.AllowSleep = false;
            InitBack();
            AddBarLayer();
            AddGround();
            AddBird();
            world.SetContactListener(new BirdContactListener(this, (CCSprite)(birdBody.UserData)));
            this.schedule(tick);

            SimpleAudioEngine.sharedEngine().playBackgroundMusic(@"musics/background", true);
            InitScore();
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 进入
        /// </summary>
        public override void onEnter()
        {
            // 注册点击事件
            CCTouchDispatcher.sharedDispatcher().addTargetedDelegate(this, 0, true);
            base.onEnter();
        }

        /// <summary>
        /// 离开
        /// </summary>
        public override void onExit()
        {
            // 移除点击事件
            CCTouchDispatcher.sharedDispatcher().removeDelegate(this);
            base.onExit();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化背景图片
        /// </summary>
        private void InitBack()
        {
            CCSprite back = CCSprite.spriteWithFile("imgs/back/back");
            back.anchorPoint = new CCPoint(0, 1);
            back.position = new CCPoint(0, AppDelegate.screenSize.height);
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
            ballBodyDef.position = new b2Vec2(AppDelegate.screenSize.width / PTM_RATIO / 2, (float)(AppDelegate.screenSize.height / PTM_RATIO));
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
            ground = CCSprite.spriteWithFile("imgs/ground/ground");
            ground.position = new CCPoint(ground.contentSize.width / 2, ground.contentSize.height / 2);

            b2BodyDef groundBodyDef = new b2BodyDef();
            groundBodyDef.position = new b2Vec2(ground.contentSize.width / PTM_RATIO / 2, ground.contentSize.height / PTM_RATIO / 2);
            groundBodyDef.userData = ground;
            groundBodyDef.type = b2BodyType.b2_staticBody;

            // 计算ground的运动，使其速度与bird的飞行速度一致
            var action1 = CCMoveTo.actionWithDuration(ground.contentSize.width / (4 * PTM_RATIO * flySpeed),
                                                      new CCPoint(ground.contentSize.width / 4, ground.position.y));
            var action2 = CCMoveTo.actionWithDuration(0, new CCPoint(ground.contentSize.width / 2, ground.position.y));
            var action = CCSequence.actionOneTwo(action1, action2);
            var repeatAction = CCRepeatForever.actionWithAction(action);
            repeatAction.tag = 0;
            ground.runAction(repeatAction);

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
            upBarBodyDef.position = new b2Vec2(AppDelegate.screenSize.width / PTM_RATIO, (AppDelegate.screenSize.height + offset + 80) / PTM_RATIO);
            upBarBodyDef.userData = upBar;
            upBarBodyDef.type = b2BodyType.b2_kinematicBody;

            b2Body upBarBody = world.CreateBody(upBarBodyDef);

            b2PolygonShape upBarBox = new b2PolygonShape();
            b2FixtureDef boxShapeDef = new b2FixtureDef();
            boxShapeDef.shape = upBarBox;
            upBarBox.SetAsBox(upBar.contentSize.width / PTM_RATIO / 2, upBar.contentSize.height / PTM_RATIO / 2);

            upBarBody.LinearVelocity = new b2Vec2(-flySpeed, 0);
            upBarBody.CreateFixture(boxShapeDef);

            barLayer.addChild(upBar);

            if (upBars == null)
            {
                upBars = new Queue<CCSprite>();
            }

            // 将新产生的对象加入到队列中
            upBars.Enqueue(upBar);

            // downBar
            CCSprite downBar = CCSprite.spriteWithFile("imgs/bar/down_bar");
            b2BodyDef downBarBodyDef = new b2BodyDef();
            downBarBodyDef.position = new b2Vec2(AppDelegate.screenSize.width / PTM_RATIO, (downBar.contentSize.height + offset * 2 - 80) / 2 / PTM_RATIO);
            downBarBodyDef.userData = downBar;
            downBarBodyDef.type = b2BodyType.b2_kinematicBody;

            b2Body downBarBody = world.CreateBody(downBarBodyDef);

            b2PolygonShape downBarBox = new b2PolygonShape();
            b2FixtureDef shapeDef = new b2FixtureDef();
            shapeDef.shape = downBarBox;
            downBarBox.SetAsBox(downBar.contentSize.width / PTM_RATIO / 2, downBar.contentSize.height / PTM_RATIO / 2);

            downBarBody.LinearVelocity = new b2Vec2(-flySpeed, 0);
            downBarBody.CreateFixture(shapeDef);
            barLayer.addChild(downBar);
        }

        /// <summary>
        /// 添加
        /// </summary>
        private void AddBarLayer()
        {
            barLayer = new CCLayer();
            this.addChild(barLayer);

            // 定时创建障碍物
            CCScheduler.sharedScheduler().scheduleSelector(AddBar, this, createBarInterval, true);
        }

        /// <summary>
        /// 初始化得分
        /// </summary>
        private void InitScore()
        {
            scoreLayer = new CCLayer();
            scoreLayer.position = new CCPoint(50, AppDelegate.screenSize.height - 50);
            this.addChild(scoreLayer);

            if (digitSprites == null)
            {
                digitSprites = new List<CCSprite>();
            }
            UpdateScore(digitSprites, scoreLayer, score, true);
        }

        /// <summary>
        /// 更新得分
        /// </summary>
        /// <param name="score"></param>
        private void UpdateScore(List<CCSprite> digitSprites, CCLayer scoreLayer, int score, bool playSound)
        {
            string scoreStr = score.ToString();

            for (int i = digitSprites.Count; digitSprites.Count < scoreStr.Length; i++)
            {
                var tempDigit = CCSprite.spriteWithFile("imgs/score/" + scoreStr[i]);
                tempDigit.position = new CCPoint(i * tempDigit.contentSize.width, 0);
                digitSprites.Add(tempDigit);
                scoreLayer.addChild(tempDigit);
            }

            // 更新
            for (int i = 0; i < scoreStr.Length; i++)
            {
                digitSprites[i].Texture = CCTextureCache.sharedTextureCache().addImage("imgs/score/" + scoreStr[i]);
            }

            if (playSound && score > 0)
            {
                SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_point");
            }
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

                    if (b.BodyType != b2BodyType.b2_staticBody)
                    {
                        sprite.position = new CCPoint((float)(b.Position.x * PTM_RATIO),
                                                      (float)(b.Position.y * PTM_RATIO));
                    }

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

            // 障碍物链表中第一个节点通过则加分
            if (upBars != null && upBars.Count > 0 && (float)upBars.First().position.x / PTM_RATIO < AppDelegate.screenSize.width / 2f / PTM_RATIO)
            {
                UpdateScore(digitSprites, scoreLayer, ++score, true);
                upBars.Dequeue();
            }
        }

        /// <summary>
        /// 点击开始
        /// </summary>
        /// <param name="pTouch"></param>
        /// <param name="pEvent"></param>
        /// <returns></returns>
        public bool ccTouchBegan(CCTouch pTouch, CCEvent pEvent)
        {
            if (isBirdAlive && birdBody.Position.y < AppDelegate.screenSize.height / PTM_RATIO)
            {
                birdBody.LinearVelocity = new b2Vec2(0, 8);
                ((CCSprite)(birdBody.UserData)).rotation = -30;
                SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_wing");
            }
            return true;
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

        /// <summary>
        /// 游戏结束
        /// </summary>
        public void GameOver()
        {
            SimpleAudioEngine.sharedEngine().stopBackgroundMusic(true);
            isBirdAlive = false;

            CCScheduler.sharedScheduler().unscheduleSelector(AddBar, this);
            var bird = (CCSprite)birdBody.UserData;
            CCMoveTo moveTo = CCMoveTo.actionWithDuration(0.5f, new CCPoint(bird.position.x, 120));
            CCRotateTo rotateTo = CCRotateTo.actionWithDuration(0.1f, 90);
            bird.runAction(moveTo);
            bird.runAction(rotateTo);
            bird.stopActionByTag(0);
            ground.stopActionByTag(0);

            if (bird.position.y > 130)
            {
                // 延迟0.2秒播放降落音效
                var tempAction = CCSequence.actions(CCDelayTime.actionWithDuration(0.2f), CCCallFunc.actionWithTarget(this, FallingDown));
                this.runAction(tempAction);
            }
            this.unschedule(tick);
            ShowGameOverMenu();
        }

        /// <summary>
        /// 降落
        /// </summary>
        private void FallingDown()
        {
            SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_die");
        }

        /// <summary>
        /// 游戏结束弹出菜单
        /// </summary>
        private void ShowGameOverMenu()
        {
            if (gameOverLayer == null)
            {
                gameOverLayer = new CCLayer();
            }

            // gameOver文字图片
            CCSprite gameOverImageSprite = CCSprite.spriteWithFile("imgs/start/gameOver");
            gameOverImageSprite.position = new CCPoint(AppDelegate.screenSize.width / 2, AppDelegate.screenSize.height * 4 / 5);
            gameOverLayer.addChild(gameOverImageSprite);

            // 得分Panel图片
            CCSprite scorePanel = CCSprite.spriteWithFile("imgs/start/scorePanel");
            scorePanel.position = new CCPoint(AppDelegate.screenSize.width / 2, AppDelegate.screenSize.height * 3 / 5);
            gameOverLayer.addChild(scorePanel);

            // 得分
            CCLayer currentScoreLayer = new CCLayer();
            List<CCSprite> digitSpriteList = new List<CCSprite>();
            currentScoreLayer.scale = 0.5f;
            currentScoreLayer.position = new CCPoint(AppDelegate.screenSize.width / 2 + 60, AppDelegate.screenSize.height * 3 / 5 + 15);
            this.addChild(currentScoreLayer);
            UpdateScore(digitSpriteList, currentScoreLayer, score, false);
            gameOverLayer.addChild(currentScoreLayer);

            // 最高分
            using (IsolatedStorageFileStream stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("/score.data", System.IO.FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StreamReader reader = new StreamReader(stream);

                string res = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(res))
                {
                    highestScore = Convert.ToInt32(res);
                }

                if (!IsolatedStorageFile.GetUserStoreForApplication().FileExists("score.data"))
                {
                    IsolatedStorageFile.GetUserStoreForApplication().CreateFile("score.data");
                }
            }
            using (IsolatedStorageFileStream stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("/score.data", System.IO.FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (score > highestScore)
                {
                    highestScore = score;
                    byte[] buffer = UTF8Encoding.UTF8.GetBytes(highestScore.ToString());
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();

                    CCSprite newHighScore = CCSprite.spriteWithFile("imgs/start/new");
                    newHighScore.position = new CCPoint(AppDelegate.screenSize.width / 2 + 35, AppDelegate.screenSize.height * 3 / 5 - 8);
                    gameOverLayer.addChild(newHighScore);
                }
            }
            CCLayer highestScoreLayer = new CCLayer();
            List<CCSprite> highestScoreDigitSpriteList = new List<CCSprite>();
            highestScoreLayer.scale = 0.5f;
            highestScoreLayer.position = new CCPoint(AppDelegate.screenSize.width / 2 + 60, AppDelegate.screenSize.height * 3 / 5 - 30);
            this.addChild(highestScoreLayer);
            UpdateScore(highestScoreDigitSpriteList, highestScoreLayer, highestScore, false);
            gameOverLayer.addChild(highestScoreLayer);


            #region 按钮

            // 开始按钮
            CCSprite btnStart = CCSprite.spriteWithFile("imgs/start/btn_start");
            CCSprite btn_startSelected = CCSprite.spriteWithFile("imgs/start/btn_startSelected");

            CCMenuItemSprite menuItemSprite1 = CCMenuItemSprite.itemFromNormalSprite(btnStart, btn_startSelected, this, (sender) =>
            {
                GameScene gameScene = new GameScene(game);

                // 跳转到下一个场景
                var scene = CCTransitionFade.transitionWithDuration(0.5f, gameScene);
                CCDirector.sharedDirector().pushScene(scene);
            });

            // 排行榜
            CCSprite btnHighScore = CCSprite.spriteWithFile("imgs/start/btn_highScore");

            CCSprite btnHighScoreSelected = CCSprite.spriteWithFile("imgs/start/btn_highScoreSelected");
            CCMenuItemSprite menuItemSprite2 = CCMenuItemSprite.itemFromNormalSprite(btnHighScore, btnHighScoreSelected, this, (sender) =>
            {
                game.Exit();
            });

            CCMenu menu = CCMenu.menuWithItems(menuItemSprite1, menuItemSprite2);
            menu.position = new CCPoint(AppDelegate.screenSize.width / 2, ground.contentSize.height + btnStart.contentSize.height / 2);

            //菜单水平排列
            menu.alignItemsHorizontallyWithPadding(20);
            this.addChild(menu);
            #endregion

            this.addChild(gameOverLayer);
        }

        #endregion
    }
}
