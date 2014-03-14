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
        #region �ֶ�

        // ��Ļ�Ŀ��
        private CCSize screenSize = CCDirector.sharedDirector().getWinSize();

        // ��Ļ����������Ĵ�С��ӳ����� 
        // ����ɲο���http://blog.csdn.net/zhangxaochen/article/details/8009508
        private float PTM_RATIO = 32.0f;

        // ��������
        private b2World world = null;

        // ������������Ϸ����bird
        private b2Body birdBody;

        private CCLayer barLayer;
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

            AddBarLayer();
            AddBar(0.5f);
            AddGround(0);
            AddBird();
            world.SetContactListener(new BirdContactListener(this, (CCSprite)(birdBody.UserData)));
            this.schedule(tick);
        }

        #endregion

        #region ��д����

        #endregion

        #region ����

        /// <summary>
        /// ��һ���ڳ��������һ����̬��bird����
        /// �ڶ�����bird�����ж���
        /// ����������box-2d����������bird�����������з���
        /// </summary>
        private void AddBird()
        {
            CCSprite bird = CCSprite.spriteWithFile("imgs/bird/bird_01");

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
            ballBodyDef.position = new b2Vec2(screenSize.width / PTM_RATIO / 2, (float)(screenSize.height / PTM_RATIO));
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

            // �����˶�����֡����
            List<CCSpriteFrame> frames = new List<CCSpriteFrame>();

            for (int i = 1; i < 3; i++)
            {
                // ֡��ͼ
                CCTexture2D texture = CCTextureCache.sharedTextureCache().addImage("imgs/ground/ground_0" + i);

                // �������һ�������bug����������õĻ����ͻᲥ�Ų���������
                texture.Name = (uint)i;
                var frame = CCSpriteFrame.frameWithTexture(texture, new CCRect(0, 0, texture.ContentSizeInPixels.width, texture.ContentSizeInPixels.height));
                frames.Add(frame);
            }

            // ����
            CCAnimation marmotShowanimation = CCAnimation.animationWithFrames(frames, 0.15f);
            CCAnimate flyAction = CCAnimate.actionWithAnimation(marmotShowanimation, false);
            CCRepeatForever repeatAction = CCRepeatForever.actionWithAction(flyAction);
            repeatAction.tag = 0;
            ground.runAction(repeatAction);

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

            CCScheduler.sharedScheduler().scheduleSelector(AddBar, this, 2f, true);
        }

        /// <summary>
        /// ���
        /// </summary>
        private void AddBarLayer()
        {
            barLayer = new CCLayer();
            this.addChild(barLayer);
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
                    sprite.rotation = -1 * MathHelper.ToDegrees(b.Angle);

                    // bird�������棬��Ϸ����
                    if (birdBody.UserData == sprite && sprite.position.x < 0)
                    {
                        CCDirector.sharedDirector().pause();
                    }

                    // ���ٲ�����Ļ�еĶ���
                    if (b.Position.x < 0)
                    {
                        sprite.removeFromParentAndCleanup(true);
                        world.DestroyBody(b);
                    }

                }
            }
        }

        public override void onEnter()
        {
            // ע�����¼�
            CCTouchDispatcher.sharedDispatcher().addTargetedDelegate(this, 0, true);
            base.onEnter();
        }

        public override void onExit()
        {
            // �Ƴ�����¼�
            CCTouchDispatcher.sharedDispatcher().removeDelegate(this);
            base.onExit();
        }

        public bool ccTouchBegan(CCTouch pTouch, CCEvent pEvent)
        {
            birdBody.LinearVelocity = new b2Vec2(0, 10);
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
