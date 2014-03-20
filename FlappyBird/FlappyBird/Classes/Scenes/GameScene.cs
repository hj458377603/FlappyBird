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
        #region �ֶ�
        private Game game;

        // ��Ļ����������Ĵ�С��ӳ����� 
        // ����ɲο���http://blog.csdn.net/zhangxaochen/article/details/8009508
        private float PTM_RATIO = 32.0f;

        // ��������
        private b2World world = null;

        // ������������Ϸ����bird
        private b2Body birdBody;

        // �ϰ����
        private CCLayer barLayer;

        // �÷ֲ�
        private CCLayer scoreLayer;

        // �÷�ֵ
        private int score = 0;

        // ��Ϸ����bird�Ƿ񻹻���
        private bool isBirdAlive = true;

        // �÷�ֵ
        private List<CCSprite> digitSprites;

        // �����ٶ�
        private float flySpeed = 3;

        // �ϰ���
        private Queue<CCSprite> upBars;

        // �ϰ������ʱ����
        private readonly float createBarInterval = 1.5f;

        // ����
        private CCSprite ground;

        // ��Ϸ����������Layer
        private CCLayer gameOverLayer;

        private int highestScore;

        #endregion

        #region ����

        #endregion

        #region ���췽��

        public GameScene(Game game)
        {
            this.game = game;
            // ��ʼ��world
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

        #region ��д����

        /// <summary>
        /// ����
        /// </summary>
        public override void onEnter()
        {
            // ע�����¼�
            CCTouchDispatcher.sharedDispatcher().addTargetedDelegate(this, 0, true);
            base.onEnter();
        }

        /// <summary>
        /// �뿪
        /// </summary>
        public override void onExit()
        {
            // �Ƴ�����¼�
            CCTouchDispatcher.sharedDispatcher().removeDelegate(this);
            base.onExit();
        }

        #endregion

        #region ����

        /// <summary>
        /// ��ʼ������ͼƬ
        /// </summary>
        private void InitBack()
        {
            CCSprite back = CCSprite.spriteWithFile("imgs/back/back");
            back.anchorPoint = new CCPoint(0, 1);
            back.position = new CCPoint(0, AppDelegate.screenSize.height);
            this.addChild(back);
        }

        /// <summary>
        /// ��һ���ڳ��������һ����̬��bird����
        /// �ڶ�����bird�����ж���
        /// ����������box-2d����������bird�����������з���
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");
            bird.rotation = -15;

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

            // �����������ж���һ��body��������λ�ã�����bird��֮��Ӧ
            b2BodyDef ballBodyDef = new b2BodyDef();
            ballBodyDef.type = b2BodyType.b2_dynamicBody;
            ballBodyDef.position = new b2Vec2(AppDelegate.screenSize.width / PTM_RATIO / 2, (float)(AppDelegate.screenSize.height / PTM_RATIO));
            ballBodyDef.userData = bird;
            birdBody = world.CreateBody(ballBodyDef);

            // Ϊbody������״��������һЩ��������
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
        /// ��һ����ӵ���
        /// �ڶ�������box-2d�������潫������Ϊ��̬������ӵ�����������
        /// </summary>
        private void AddGround()
        {
            ground = CCSprite.spriteWithFile("imgs/ground/ground");
            ground.position = new CCPoint(ground.contentSize.width / 2, ground.contentSize.height / 2);

            b2BodyDef groundBodyDef = new b2BodyDef();
            groundBodyDef.position = new b2Vec2(ground.contentSize.width / PTM_RATIO / 2, ground.contentSize.height / PTM_RATIO / 2);
            groundBodyDef.userData = ground;
            groundBodyDef.type = b2BodyType.b2_staticBody;

            // ����ground���˶���ʹ���ٶ���bird�ķ����ٶ�һ��
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
        /// ����ڵ��ϰ�
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

            // ���²����Ķ�����뵽������
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
        /// ���
        /// </summary>
        private void AddBarLayer()
        {
            barLayer = new CCLayer();
            this.addChild(barLayer);

            // ��ʱ�����ϰ���
            CCScheduler.sharedScheduler().scheduleSelector(AddBar, this, createBarInterval, true);
        }

        /// <summary>
        /// ��ʼ���÷�
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
        /// ���µ÷�
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

            // ����
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
        /// ģ����������
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

                    // ���ٲ�����Ļ�еĶ���
                    if (b.Position.x < -3)
                    {
                        sprite.removeFromParentAndCleanup(true);
                        world.DestroyBody(b);
                    }

                }
            }

            // �ϰ��������е�һ���ڵ�ͨ����ӷ�
            if (upBars != null && upBars.Count > 0 && (float)upBars.First().position.x / PTM_RATIO < AppDelegate.screenSize.width / 2f / PTM_RATIO)
            {
                UpdateScore(digitSprites, scoreLayer, ++score, true);
                upBars.Dequeue();
            }
        }

        /// <summary>
        /// �����ʼ
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
        /// ��Ϸ����
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
                // �ӳ�0.2�벥�Ž�����Ч
                var tempAction = CCSequence.actions(CCDelayTime.actionWithDuration(0.2f), CCCallFunc.actionWithTarget(this, FallingDown));
                this.runAction(tempAction);
            }
            this.unschedule(tick);
            ShowGameOverMenu();
        }

        /// <summary>
        /// ����
        /// </summary>
        private void FallingDown()
        {
            SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_die");
        }

        /// <summary>
        /// ��Ϸ���������˵�
        /// </summary>
        private void ShowGameOverMenu()
        {
            if (gameOverLayer == null)
            {
                gameOverLayer = new CCLayer();
            }

            // gameOver����ͼƬ
            CCSprite gameOverImageSprite = CCSprite.spriteWithFile("imgs/start/gameOver");
            gameOverImageSprite.position = new CCPoint(AppDelegate.screenSize.width / 2, AppDelegate.screenSize.height * 4 / 5);
            gameOverLayer.addChild(gameOverImageSprite);

            // �÷�PanelͼƬ
            CCSprite scorePanel = CCSprite.spriteWithFile("imgs/start/scorePanel");
            scorePanel.position = new CCPoint(AppDelegate.screenSize.width / 2, AppDelegate.screenSize.height * 3 / 5);
            gameOverLayer.addChild(scorePanel);

            // �÷�
            CCLayer currentScoreLayer = new CCLayer();
            List<CCSprite> digitSpriteList = new List<CCSprite>();
            currentScoreLayer.scale = 0.5f;
            currentScoreLayer.position = new CCPoint(AppDelegate.screenSize.width / 2 + 60, AppDelegate.screenSize.height * 3 / 5 + 15);
            this.addChild(currentScoreLayer);
            UpdateScore(digitSpriteList, currentScoreLayer, score, false);
            gameOverLayer.addChild(currentScoreLayer);

            // ��߷�
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


            #region ��ť

            // ��ʼ��ť
            CCSprite btnStart = CCSprite.spriteWithFile("imgs/start/btn_start");
            CCSprite btn_startSelected = CCSprite.spriteWithFile("imgs/start/btn_startSelected");

            CCMenuItemSprite menuItemSprite1 = CCMenuItemSprite.itemFromNormalSprite(btnStart, btn_startSelected, this, (sender) =>
            {
                GameScene gameScene = new GameScene(game);

                // ��ת����һ������
                var scene = CCTransitionFade.transitionWithDuration(0.5f, gameScene);
                CCDirector.sharedDirector().pushScene(scene);
            });

            // ���а�
            CCSprite btnHighScore = CCSprite.spriteWithFile("imgs/start/btn_highScore");

            CCSprite btnHighScoreSelected = CCSprite.spriteWithFile("imgs/start/btn_highScoreSelected");
            CCMenuItemSprite menuItemSprite2 = CCMenuItemSprite.itemFromNormalSprite(btnHighScore, btnHighScoreSelected, this, (sender) =>
            {
                game.Exit();
            });

            CCMenu menu = CCMenu.menuWithItems(menuItemSprite1, menuItemSprite2);
            menu.position = new CCPoint(AppDelegate.screenSize.width / 2, ground.contentSize.height + btnStart.contentSize.height / 2);

            //�˵�ˮƽ����
            menu.alignItemsHorizontallyWithPadding(20);
            this.addChild(menu);
            #endregion

            this.addChild(gameOverLayer);
        }

        #endregion
    }
}
