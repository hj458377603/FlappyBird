using Box2D.Dynamics;
using cocos2d;
using CocosDenshion;
using FlappyBird.Classes.Scenes;

namespace FlappyBird.Classes.ContactListeners
{
    public class BirdContactListener : b2ContactListener
    {
        #region 字段

        private CCScene scene;
        private CCNode bird;

        #endregion


        #region 构造方法

        public BirdContactListener(CCScene scene, CCNode bird)
        {
            this.scene = scene;
            this.bird = bird;
        }

        #endregion

        public override void BeginContact(Box2D.Dynamics.Contacts.b2Contact contact)
        {
            if (contact.FixtureA.Body.UserData == bird || contact.FixtureB.Body.UserData == bird)
            {

                SimpleAudioEngine.sharedEngine().playEffect(@"musics/sfx_hit");
                //游戏结束
                ((GameScene)scene).GameOver();
            }
        }

        public override void PostSolve(Box2D.Dynamics.Contacts.b2Contact contact, ref b2ContactImpulse impulse)
        {
            //throw new NotImplementedException();
        }

        public override void PreSolve(Box2D.Dynamics.Contacts.b2Contact contact, Box2D.Collision.b2Manifold oldManifold)
        {
            //throw new NotImplementedException();
        }
    }
}
