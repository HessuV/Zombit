using System;
using System.Collections.Generic;
using System.Threading;
using FontStashSharp.RichText;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Effects;
using Timer = Jypeli.Timer;

namespace Zombit;

/// @author Heikki Vuorio, Disa Vuorio
/// @version 29.12.2023
/// <summary>
/// 
/// </summary>

public class Zombit : PhysicsGame
{
    private const double NOPEUS = 1000; 
    private double suunta = -5;
    private double tuhoamisX;
    private const int RUUDUN_KOKO = 100;
    private PhysicsObject pahis1;
    private Image pahiksenKuva = LoadImage("Peruszombi.png");
    private PhysicsObject pahis2;
    private Image pahiksenKuva2 = LoadImage("Naiszombi.png");
    private PhysicsObject pahis3;
    private Image pahiksenKuva3 = LoadImage("Isozombi.png");
    private PhysicsObject pahis4;
    private Image pahiksenKuva4 = LoadImage("Mummozombi.png");
    private PhysicsObject pelaaja1;
    private Image pelaajanKuva = LoadImage("survivor.png");
    private IntMeter pelaajan1Pisteet;
    private AssaultRifle pelaajan1Ase;
    private Grenade kranu;
    private PhysicsObject ammus;
    private IntMeter Pelaajan1Pisteet;
    private DoubleMeter elamanlaskuri;
    private Image taustakuva = LoadImage("tausta1.png");
    private Timer ajastin;
    private EasyHighScore ParhaatPisteet = new EasyHighScore();
    private Image pauto = LoadImage("PunainenAuto.png");
    private Image kAuto = LoadImage("Kuorkki.png");
    private Image tankki = LoadImage("Tankki.png");
    private Image bpossu = LoadImage("Betonipossut.png");
    private Image puita = LoadImage("puut.png");
    
    
    
    public override void Begin()
    {
        LuoKentta();
        AloitaPeli();
        LuoElamanLaskuri();
        LisaaLaskuri();
        LisaaPelaajan1Ase();
        LisaaOhjaimet();
        LuoAlkuvalikko();
        LisaaPahis2();
        LisaaPahis3();
        LisaaPahis4();
        LisaaPahis1();
        //PelaajaKuoli();
       
        tuhoamisX = Level.Left;
        
        Timer.CreateAndStart(3.0, LisaaPahis2);
        Timer.CreateAndStart(3.0, LisaaPahis4);
        Timer.CreateAndStart(3.0, LisaaPahis3);
        Timer.CreateAndStart(3.0, LisaaPahis1);

    }

    private void LuoKentta()
    {
       
        TileMap kentta = TileMap.FromLevelAsset("zombitkentta.txt");
        kentta.SetTileMethod('K', LisaaReuna);
        kentta.SetTileMethod('A', LisaaAuto);
        kentta.SetTileMethod('R', LisaaKauto);
        kentta.SetTileMethod('X', LisaaTankki);
        kentta.SetTileMethod('B', LisaaBpossu);
        kentta.SetTileMethod('T', LisaaTolppa);
        kentta.SetTileMethod('N', LisaaPelaaja1);
        kentta.SetTileMethod('P', LisaaPuu);
        kentta.Execute(RUUDUN_KOKO,RUUDUN_KOKO);
        Level.CreateBorders();
        ///Level.Background.CreateGradient(Color.Gray, Color.DarkGray);
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = false;
        Camera.FollowOffset = new Vector(Screen.Width / 2.5 - RUUDUN_KOKO, 0.0);
        Level.Background.Image = taustakuva;
        Level.Background.FitToLevel();
        
        
    }

    void AloitaPeli()
    {
       
    }

    void LuoAlkuvalikko()
    {
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Pelin alkuvalikko", "Aloita peli", "Parhaat pisteet", "Lopeta");
        Add(alkuvalikko);
        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(2, Exit);
        ///alkuvalikko.AddItemHandler(1, ParhaatPisteet);
        alkuvalikko.DefaultCancel = 3;
        alkuvalikko.Color = Color.ForestGreen;
        alkuvalikko.SetButtonColor(Color.Red);
        alkuvalikko.SetButtonTextColor(Color.Black);
    }

    void LisaaOhjaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, HeitaKranaatti, "Heitä kranaatti", pelaaja1);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "ammu", pelaajan1Ase);
        Keyboard.Listen(Key.A, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta vasemmalle",new Vector(-1000, 0));
        Keyboard.Listen(Key.S, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta alas", new Vector(0, -1000));
        Keyboard.Listen(Key.D, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta oikealle", new Vector(1000, 0));
        Keyboard.Listen(Key.W, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta ylös", new Vector (0, 1000));
        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli"); 
        
    }

   
    
    void LisaaPahis1()
    {
        PhysicsObject pahis1 = new PhysicsObject(60, 60);
        pahis1.Image = pahiksenKuva;
        pahis1.Restitution = 0;
        pahis1.CanRotate = false;
        pahis1.Tag = "vihu";
        pahis1.X = pahis1.X + -800;
        pahis1.Y = pahis1.Y + 400;
        pahis1.Move(new Vector());
        Add(pahis1);
        
        Timer ajastin = new Timer();
        ajastin.Interval = 3.5;
        ajastin.Start();
        
        RandomMoverBrain satunnaisaivot = new RandomMoverBrain(100);
        satunnaisaivot.ChangeMovementSeconds = 3;
        pahis1.Brain = satunnaisaivot;
        
        FollowerBrain seuraajanAivot = new FollowerBrain(pelaaja1);
        seuraajanAivot.Speed = 300;
        seuraajanAivot.DistanceFar = 600;
        seuraajanAivot.FarBrain = satunnaisaivot;
        

    }
    
    void LisaaPahis2()
    {
        pahis2 = new PhysicsObject(60, 60);
        pahis2.Image = pahiksenKuva2;
        pahis2.Restitution = 0;
        pahis2.CanRotate = false;
        pahis2.Tag = "vihu";
        pahis2.X = pahis2.X + -600;
        pahis2.Y = pahis2.Y -400;
        pahis2.Move(new Vector());
        Add(pahis2);
        
        RandomMoverBrain satunnaisaivot = new RandomMoverBrain(100);
        satunnaisaivot.ChangeMovementSeconds = 3;
        pahis2.Brain = satunnaisaivot;
        
        FollowerBrain seuraajanAivot = new FollowerBrain(pelaaja1);
        seuraajanAivot.Speed = 300;
        seuraajanAivot.DistanceFar = 600;
        seuraajanAivot.FarBrain = satunnaisaivot;
        
    }

    void LisaaPahis3()
    {
        pahis3 = new PhysicsObject(60, 60);
        pahis3.Image = pahiksenKuva3;
        pahis3.Restitution = 0;
        pahis3.CanRotate = false;
        pahis3.Tag = "vihu";
        pahis3.X = pahis3.X + 800;
        pahis3.Y = pahis3.Y + 400;
        pahis3.Move(new Vector());
        Add(pahis3);
        
        RandomMoverBrain satunnaisaivot = new RandomMoverBrain(100);
        satunnaisaivot.ChangeMovementSeconds = 3;
        pahis3.Brain = satunnaisaivot;
        
        FollowerBrain seuraajanAivot = new FollowerBrain(pelaaja1);
        seuraajanAivot.Speed = 300;
        seuraajanAivot.DistanceFar = 600;
        seuraajanAivot.FarBrain = satunnaisaivot;
        
    }

    void LisaaPahis4()
    {
        pahis4 = new PhysicsObject(60, 60);
        pahis4.Image = pahiksenKuva4;
        pahis4.Restitution = 0;
        pahis4.CanRotate = false;
        pahis4.Tag = "vihu";
        pahis4.Move(new Vector());
        pahis4.X = pahis4.X + 1000;
        pahis4.Y = pahis4.Y + 100;
        Add(pahis4);
        
        RandomMoverBrain satunnaisaivot = new RandomMoverBrain(100);
        satunnaisaivot.ChangeMovementSeconds = 3;
        pahis4.Brain = satunnaisaivot;
        
        FollowerBrain seuraajanAivot = new FollowerBrain(pelaaja1);
        seuraajanAivot.Speed = 300;
        seuraajanAivot.DistanceFar = 600;
        seuraajanAivot.FarBrain = satunnaisaivot;
        
    }

   
    void LisaaPelaaja1(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PhysicsObject(60, 60);
        pelaaja1.Image = pelaajanKuva;
        pelaaja1.Restitution = 0;
        pelaaja1.Position = paikka;
        pelaaja1.CanRotate = false;
        pelaaja1.IgnoresPhysicsLogics = true;
        pelaaja1.LinearDamping = 1.0;
        pelaaja1.IgnoresExplosions = true;
        pelaaja1.X = pelaaja1.X + -400;
        pelaaja1.Y = pelaaja1.Y - 100;
        Add(pelaaja1);
        AddCollisionHandler(pelaaja1, "vihu", tormaaViholliseen);
    }

    
    void LisaaPelaajan1Ase()
    {
        pelaajan1Ase = new AssaultRifle(40, 20);
        pelaajan1Ase.Ammo.Value = 1000;
        pelaajan1Ase.FireRate = 4;
        pelaajan1Ase.ProjectileCollision = AmmusOsui;
        pelaajan1Ase.Position = pelaaja1.Position;
        pelaajan1Ase.CanHitOwner = false;
        pelaaja1.Add(pelaajan1Ase);
    }

  
    void HeitaKranaatti(PhysicsObject pelaaja1)
    {
        Grenade kranu = new Grenade(10.0);
        kranu.Explosion.AddShockwaveHandler("vihu", KranaattiOsui);
        pelaaja1.Throw(kranu, Angle.FromDegrees(20), 100);
    }
    
    
    void KranaattiOsui(IPhysicsObject kohde, Vector v)
    {
        kohde.Destroy();
        
        if (kohde.Tag.ToString() == "vihu")
        {
            pelaajan1Pisteet.Value += 1;
        }
       
    }
    
    void LiikutaPelaajaa (Vector vektori)
    {
        pelaaja1.Push(vektori);
    }
 
    
    
    private void LisaaReuna(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject reuna = PhysicsObject.CreateStaticObject(100,100);
        reuna.Position = paikka;
        reuna.Color = Color.Gray;
        Add(reuna);
    }

    
    public void LisaaAuto(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject auto = PhysicsObject.CreateStaticObject(60, 60);
        auto.Position = paikka;
        auto.Image = pauto;
        Add(auto);
    }       

    public void LisaaKauto(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kauto = PhysicsObject.CreateStaticObject(60,60);
        kauto.Position = paikka;
        kauto.Image = kAuto;
        Add(kauto);
    }
    
    public void LisaaTankki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tank = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tank.Position = paikka;
        tank.Image = tankki;
        Add(tank);
    }
    
    public void LisaaBpossu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject bpossut = PhysicsObject.CreateStaticObject(leveys, korkeus);
        bpossut.Position = paikka;
        bpossut.Image = bpossu;
        Add(bpossut);
    }
    
    public void LisaaPuu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject puu = PhysicsObject.CreateStaticObject(leveys, korkeus);
        puu.Position = paikka;
        puu.Image = puita;
        Add(puu);
    }
    
    
    
    
    private void LisaaTolppa(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tolppa = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tolppa.Position = paikka;
        tolppa.Color = Color.Gray;
        Add(tolppa);
    }

    void LisaaLaskuri()
    {
        pelaajan1Pisteet = LuoPisteLaskuri(Screen.Left + 100.0, Screen.Top - 50.0);
    }

    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);
        laskuri.MaxValue = 100;
        Label naytto = new Label(100,100);
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.White;
        naytto.BorderColor = Color.Black;
        naytto.Color = Color.Black;
        Add(naytto);

        return laskuri;
    }
    
    
    
    void AmmuAseella(AssaultRifle ase)
    {
        PhysicsObject ammus = ase.Shoot();

        if (ammus != null)
        {
            ammus.Size *= 1;
            ammus.MaximumLifetime =TimeSpan.FromSeconds(2.0);
        }
    }
    
    void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.Destroy();
        
        if (kohde.Tag.ToString() == "vihu")
        {
            pelaajan1Pisteet.Value += 1;
        }
       

    }

    void LuoElamanLaskuri()
    {
        elamanlaskuri = new DoubleMeter(10);
        elamanlaskuri.MaxValue = 10;
        elamanlaskuri.LowerLimit += ElamaLoppui;
        
        ProgressBar elamapalkki = new ProgressBar(150, 20);
        elamapalkki.X = Screen.Right - 150;
        elamapalkki.Y = Screen.Top - 30;
        elamapalkki.BindTo(elamanlaskuri);
        Add(elamapalkki);
        elamapalkki.Color = Color.Black;
        elamapalkki.BarColor = Color.Red;
        elamapalkki.BorderColor = Color.Black;

    }

    void tormaaViholliseen(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        elamanlaskuri.Value -= 1;
    }
    void ElamaLoppui()
    {
            MessageDisplay.Add("kuolit!");
            Keyboard.Disable(Key.A);
            Keyboard.Disable(Key.W);
            Keyboard.Disable(Key.S);
            Keyboard.Disable(Key.D);
            Keyboard.Disable(Key.Space);
            Keyboard.Disable(Key.Tab);
    }

    //void PelaajaKuoli()
    //{
    //    pelaaja1.Destroy();
    //    ParhaatPisteet.EnterAndShow(pelaajan1Pisteet.Value);
    //    ParhaatPisteet.HighScoreWindow.Closed += AloitaPeli;
    //}


    
}
