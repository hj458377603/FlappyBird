using Box2D.Collision.Shapes;
using Box2D.Common;
using Box2D.Dynamics;
using cocos2d;
using CocosDenshion;
using FlappyBird.Classes.ContactListeners;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsPhoneGame2.Classes;

namespace FlappyBird.Classes.Scenes
{
    class GameScene : CCScene, ICCTargetedTouchDelegate
    {
        #region �ֶ�

        // ��Ļ����������Ĵ�С��ӳ����� 
        // ����ɲο���http://blog.csdn.net/zhangxaochen/article/details/8009508
        private float PTM_RATIO = 32.0f;

        // ��������
        private b2World world = null;

        // ������������Ϸ����bird
        private b2Body birdBody;

        // �ϰ����
        private CCLayer barLayer;

        // �÷�ֵ
        private int score = 0;

        // ��Ϸ����bird�Ƿ񻹻���
        private bool isBirdAlive = true;

        // �÷ֲ�
        private CCLayer scoreLayer;

        // �÷�ֵ
        private List<CCSprite> digitSprites;

        // �����ٶ�
        private float flySpeed = 3;

        // �ϰ���
        private LinkedList<CCSprite> upBars;

        // �ϰ������ʱ����
        private readonly float createBarInterval = 1.5f;

        #endregion

        #region ����

        #endregion

        #region ���췽��

        public GameScene()
        {
            // ��ʼ��world
            b2Vec2 gravity = new b2Vec2(0f, -20f);
            world = new b2World(gravity);
            world.AllowSleep = false;
            InitBack();
            AddBarLayer();
            AddGround(0);
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
                upBars = new LinkedList<CCSprite>();
            }
            if (upBars.Count == 0)
            {
                upBars.AddFirst(upBar);
            }
            else
            {
                upBars.AddAfter(upBars.Last, new LinkedListNode<CCSprite>(upBar));
            }

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
            UpdateScore(score);
        }

        /// <summary>
        /// ���µ÷�
        /// </summary>
        /// <param name="score"></param>
        private void UpdateScore(int score)
        {
            string scoreStr = score.ToString();

            if (digitSprites == null)
            {
                digitSprites = new List<CCSprite>();
            }

            if (digitSprites.Count < scoreStr.Length)
            {
                var tempDigit = CCSprite.spriteWithFile("imgs/score/" + scoreStr[0]);
                tempDigit.position = new CCPoint(20 * (scoreStr.Length - 1), 0);
                digitSprites.Add(tempDigit);
                scoreLayer.addChild(tempDigit);
            }

            // ����
            for (int i = 0; i < scoreStr.Length; i++)
            {
                digitSprites[i].Texture = CCTextureCache.sharedTextureCache().addImage("imgs/score/" + scoreStr[i]);
            }

            if (score > 0)
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
                    sprite.position = new CCPoint((float)(b.Position.x * PTM_RATIO),
                                                  (float)(b.Position.y * PTM_RATIO));

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
            if (upBars != null && upBars.First != null && (float)upBars.First().position.x / PTM_RATIO < AppDelegate.screenSize.width / 2f / PTM_RATIO)
            {
                UpdateScore(++score);
                upBars.RemoveFirst();
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
            if (isBirdAlive)
            {
                birdBody.LinearVelocity = new b2Vec2(0, 8);
                ((CCSprite)(birdBody.UserData)).rotation = -30;
                SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_wing");
            }
            else
            {
                var scene = CCTransitionFade.transitionWithDuration(0.5f, new GameScene());
                CCDirector.sharedDirector().pushScene(scene);
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

            if (bird.position.y > 130)
            {
                // �ӳ�0.2�벥�Ž�����Ч
                var tempAction = CCSequence.actions(CCDelayTime.actionWithDuration(0.2f), CCCallFunc.actionWithTarget(this, FallingDown));
                this.runAction(tempAction);
            }
            this.unschedule(tick);
        }

        /// <summary>
        /// ����
        /// </summary>
        private void FallingDown()
        {
            SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_die");
        }
        #endregion
    }
}
