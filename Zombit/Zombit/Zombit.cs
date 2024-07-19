using System;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;
using Jypeli.Effects;
using Timer = Jypeli.Timer;

namespace Zombit;

/// @author Heikki Vuorio, Disa Vuorio
/// Heikin taulukko for-silmukalla: https://tim.jyu.fi/answers/kurssit/tie/ohj1/avoin23/demot/demo5?answerNumber=2&amp;task=taulukot5&amp;user=hejumivu
/// Disan taulukko for-silmukalla: https://tim.jyu.fi/answers/kurssit/tie/ohj1/avoin23/demot/demo11?answerNumber=14&amp;task=taulukot2&amp;user=dikavuor
/// @version 29.12.2023
/// <summary>
/// Peli, jossa on maailmanloppu ja selviytyjän tehtävänä on selviytyä laiturille, jotta voi purjehtia turvaan.
/// </summary>
public class Zombit : PhysicsGame
{
    private const int RuudunKoko = 200;

    private Image[] _kuvat =
        LoadImages("Peruszombi", "Naiszombi", "Isozombi", "Mummozombi", "zombipupu", "hyttyszombi");

    private PhysicsObject _pelaaja1;
    private Image[] _survivorKavely = LoadImages("survivorJuoksee", "survivorseisoo");
    private Image _pelaajanKuva = LoadImage("survivor.png");
    private Image _pelaajanKuolemaKuva = LoadImage("survivorkuoli");
    private IntMeter _pelaajan1Pisteet;
    private AssaultRifle _pelaajan1Ase;
    private DoubleMeter _elamanlaskuri;
    private Image _taustakuva = LoadImage("tausta1.png");
    private EasyHighScore _parhaatPisteet = new EasyHighScore();
    private Image _pauto = LoadImage("PunainenAuto.png");
    private Image _kAuto = LoadImage("Kuorkki.png");
    private Image _tankki = LoadImage("Tankki.png");
    private Image _bpossu = LoadImage("Betonipossut.png");
    private Image _puita = LoadImage("puut.png");
    private Image _tuli = LoadImage("tulivuori");
    private Image _hela = LoadImage("heladrinksu");
    private Image _beissi = LoadImage("talo");
    private Image _laava = LoadImage("laacaa");
    private Image _musta = LoadImage("kappana");
    private Image _maali = LoadImage("lankku");
    private Image _pommit = LoadImage("pommi");


    public override void Begin()
    {
        LuoAlkuvalikko();
    }


    /// <summary>
    /// Luodaan peliin kenttä.
    /// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("zombitkentta.txt");
        kentta.SetTileMethod('K', LisaaReuna);
        kentta.SetTileMethod('A', LisaaAuto);
        kentta.SetTileMethod('R', LisaaKauto);
        kentta.SetTileMethod('X', LisaaTankki);
        kentta.SetTileMethod('O', LisaaBpossu);
        kentta.SetTileMethod('N', LisaaPelaaja1);
        kentta.SetTileMethod('P', LisaaPuu);
        kentta.SetTileMethod('B', LisaaBeissi);
        kentta.SetTileMethod('L', LisaaLaava);
        kentta.SetTileMethod('M', LisaaKappana);
        kentta.SetTileMethod('U', LisaaTuli);
        kentta.SetTileMethod('H', LisaaHela);
        kentta.SetTileMethod('J', LisaaPommi);
        kentta.SetTileMethod('D', LisaaSavu);
        kentta.SetTileMethod('W', LisaaMaali);
        kentta.Execute(RuudunKoko, RuudunKoko);
        Level.CreateBorders();
        Camera.Follow(_pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = false;
        Camera.FollowOffset = new Vector(Screen.Width / 2.5 - RuudunKoko, 0.0);
        Level.Background.Image = _taustakuva;
        Level.Background.FitToLevel();
        MessageWindow ikkuna =
            new MessageWindow(
                " Hei Selviytyjä! Maapallon ovat vallanneet Zombit. Vene odottaa sinua satamassa, sen avulla pääset turvaan. Onnea matkaan! -Ystäväsi Ryan");
        ikkuna.Color = Color.White;
        Add(ikkuna);
    }


    /// <summary>
    /// Luodaan peliin aloita peli aliohjelma, joka nollaa kentän ja luo sinne uudelleen kentän ominaisuudet.
    /// </summary>
    void AloitaPeli()
    {
        //MediaPlayer.Play("ZombiesAreComing");
        ClearAll();
        LuoKentta();
        LuoElamanLaskuri();
        LisaaLaskuri();
        LisaaPelaajan1Ase();
        LisaaOhjaimet();
        LisaaZombit();
        Timer.CreateAndStart(10.0, LisaaZombit);
    }


    /// <summary>
    /// Luodaan peliin alkuvalikko, josta on mahdollisuus valita: aloita peli, parhaat pisteet ja lopeta peli.
    /// </summary>
    void LuoAlkuvalikko()
    {
        ClearAll();
        MultiSelectWindow alkuvalikko =
            new MultiSelectWindow("Pelin alkuvalikko", "Aloita peli", "Parhaat pisteet", "Lopeta");
        Add(alkuvalikko);
        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(1, NaytaParhaatPisteet);
        alkuvalikko.AddItemHandler(2, Exit);
        alkuvalikko.DefaultCancel = 3;
        alkuvalikko.Color = Color.LimeGreen;
        alkuvalikko.SetButtonColor(Color.White);
        alkuvalikko.SetButtonTextColor(Color.Black);
    }


    /// <summary>
    /// Luodaan aliohjelma Pausella, joka pysäyttää pelin väliaikaisesti.
    /// </summary>
    void Pausella()
    {
        IsPaused = !IsPaused;
    }


    /// <summary>
    /// Luodaan parhaat pisteet alkuvalikkoon.
    /// </summary>
    void NaytaParhaatPisteet()
    {
        _parhaatPisteet.Show();
        _parhaatPisteet.HighScoreWindow.Closed += delegate { LuoAlkuvalikko(); };
    }


    /// <summary>
    /// Luodaan peliin ohjaimet, joilla ohjataan selviytyjää sekä voidaan ampua aseella ja heittää kranaatteja.
    /// </summary>
    void LisaaOhjaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, HeitaKranaatti, "Heitä kranaatti", _pelaaja1);
        Keyboard.Listen(Key.Space, ButtonState.Down, AmmuAseella, "ammu", _pelaajan1Ase);
        Keyboard.Listen(Key.A, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta vasemmalle", new Vector(-500, 0));
        Keyboard.Listen(Key.A, ButtonState.Released, LiikutaPelaajaa, "Pelaaja 1: Pysähdy", new Vector(0, 0));
        Keyboard.Listen(Key.S, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta alas", new Vector(0, -500));
        Keyboard.Listen(Key.S, ButtonState.Released, LiikutaPelaajaa, "Pelaaja 1: Pysähdy", new Vector(0, 0));
        Keyboard.Listen(Key.D, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta oikealle", new Vector(500, 0));
        Keyboard.Listen(Key.D, ButtonState.Released, LiikutaPelaajaa, "Pelaaja 1: Pysähdy", new Vector(0, 0));
        Keyboard.Listen(Key.W, ButtonState.Down, LiikutaPelaajaa, "Pelaaja 1: Liikuta ylös", new Vector(0, 500));
        Keyboard.Listen(Key.W, ButtonState.Released, LiikutaPelaajaa, "Pelaaja 1: Pysähdy", new Vector(0, 0));
        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.U, ButtonState.Pressed, AloitaUudelleen, "aloita peli alusta");
        Keyboard.Listen(Key.P, ButtonState.Pressed, Pausella, "peli pauselle");
    }


    /// <summary>
    /// Luodaan peliin pelaaja.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    private void LisaaPelaaja1(Vector paikka, double leveys, double korkeus)
    {
        _pelaaja1 = new PhysicsObject(60, 60)
        {
            Image = _pelaajanKuva,
            Animation = new Animation(_survivorKavely)
            {
                FPS = 5
            },
            Restitution = 0,
            Position = paikka,
            CanRotate = false,
            IgnoresPhysicsLogics = true,
            LinearDamping = 1.0,
            IgnoresExplosions = true
        };
        Add(_pelaaja1);

        _pelaaja1.Animation.Start();

        AddCollisionHandler(_pelaaja1, "vihu", TormaaViholliseen);
        AddCollisionHandler(_pelaaja1, "hela", PelaajaParantuu);
        AddCollisionHandler(_pelaaja1, "maali", TormaaMaaliin);
        AddCollisionHandler(_pelaaja1, "pommi", TormaaPommiin);
    }


    /// <summary>
    /// Luodaan nappula, josta voi aloittaa pelin uudelleen.
    /// </summary>
    void AloitaUudelleen()
    {
        //MediaPlayer.Play("ZombiesAreComing");
        ClearAll();
        LuoKentta();
        LuoElamanLaskuri();
        LisaaLaskuri();
        LisaaPelaajan1Ase();
        LisaaOhjaimet();
        LisaaZombit();
    }


    /// <summary>
    /// Luodaan peliin zombeja ja niiden ominaisuudet.
    /// </summary>
    /// <param name="kuvia">kuvat</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    void LisaaPahis(Image kuvia, int x, int y)
    {
        PhysicsObject pahis1 = new PhysicsObject(60, 60);
        pahis1.Image = kuvia;
        pahis1.Restitution = 0;
        pahis1.CanRotate = false;
        pahis1.IgnoresCollisionResponse = true;
        pahis1.Tag = "vihu";
        pahis1.X = pahis1.X + x;
        pahis1.Y = pahis1.Y + y;
        pahis1.Move(new Vector());
        Add(pahis1);

        RandomMoverBrain satunnaisaivot = new RandomMoverBrain(100);
        satunnaisaivot.ChangeMovementSeconds = 3;
        pahis1.Brain = satunnaisaivot;

        FollowerBrain seuraajanAivot = new FollowerBrain(_pelaaja1);
        seuraajanAivot.Speed = 300;
        seuraajanAivot.DistanceFar = 600;
        seuraajanAivot.FarBrain = satunnaisaivot;
    }

    /// <summary>
    /// Aliohjelma lisää zombeja taulukosta kentälle.
    /// </summary>
    void LisaaZombit()
    {
        for (int i = 0; i < 6; i++)
        {
            LisaaPahis(_kuvat[i], -300 + i * 200, 100);
        }
    }


    /// <summary>
    /// Kun pelaaja pääsee maaliin, tulee teksti ruutuun, että pääsit läpi.
    /// </summary>
    /// <param name="pelaaja1">pelaaja1</param>
    /// <param name="kohde">kohde</param>
    void TormaaMaaliin(PhysicsObject pelaaja1, PhysicsObject kohde)
    {
        if (kohde.Tag.ToString() == "maali")
        {
            Label tekstikentt = new Label(350, 150, "PÄÄSIT KENTÄN LÄPI! WUHUU!");
            tekstikentt.X = tekstikentt.X + 0;
            tekstikentt.Y = tekstikentt.Y + 200;
            tekstikentt.Color = Color.LimeGreen;
            tekstikentt.TextColor = Color.Black;
            tekstikentt.BorderColor = Color.White;
            MediaPlayer.Play("fanfaari");
            Add(tekstikentt);

            _parhaatPisteet.EnterAndShow(_pelaajan1Pisteet.Value);
            _parhaatPisteet.HighScoreWindow.Closed += delegate { AloitaPeli(); };
        }
    }


    /// <summary>
    /// Luodaan kenttään maaliviiva.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lankku = PhysicsObject.CreateStaticObject(250, 200);
        lankku.Position = paikka;
        lankku.Tag = "maali";
        lankku.Image = _maali;
        Add(lankku);
    }


    /// <summary>
    /// Luodaan peliin pelaajalle ase. 
    /// </summary>
    void LisaaPelaajan1Ase()
    {
        _pelaajan1Ase = new AssaultRifle(40, 20);
        _pelaajan1Ase.Ammo.Value = 1000;
        _pelaajan1Ase.FireRate = 4;
        _pelaajan1Ase.ProjectileCollision = AmmusOsui;
        _pelaajan1Ase.Position = _pelaaja1.Position;
        _pelaajan1Ase.CanHitOwner = false;
        _pelaaja1.Add(_pelaajan1Ase);
    }


    /// <summary>
    /// Aliohjelmassa määritetään ammuksen ominaisuuksia.
    /// </summary>
    /// <param name="ase">ase</param>
    void AmmuAseella(AssaultRifle ase)
    {
        PhysicsObject ammus = ase.Shoot();

        if (ammus != null)
        {
            ammus.Size *= 1;
            ammus.MaximumLifetime = TimeSpan.FromSeconds(2.0);
        }
    }


    /// <summary>
    /// kun ammus osuu zombiin, pelaaja saa pisteen ja ammus tuhoutuu.
    /// </summary>
    /// <param name="ammus">ammus</param>
    /// <param name="kohde">kohde</param>
    void AmmusOsui(IPhysicsObject ammus, IPhysicsObject kohde)
    {
        ammus.Destroy();
        //
        if (kohde.Tag.ToString() == "vihu")
        {
            kohde.Destroy();
            _pelaajan1Pisteet.Value += 1;
        }
    }


    /// <summary>
    /// Luodaan kranaatti, jota pelaaja voi heittää zombeja kohti.
    /// </summary>
    /// <param name="pelaaja1">pelaaja1</param>
    void HeitaKranaatti(PhysicsObject pelaaja1)
    {
        Grenade kranu = new Grenade(10.0);
        kranu.Explosion.AddShockwaveHandler("vihu", KranaattiOsui);
        pelaaja1.Throw(kranu, Angle.FromDegrees(20), 3000);
    }


    /// <summary>
    /// Kun kranaatti osuu zombiin, pelaaja saa pisteen ja zombi tuhoutuu.
    /// </summary>
    /// <param name="kohde">kohde</param>
    /// <param name="v">v</param>
    void KranaattiOsui(IPhysicsObject kohde, Vector v)
    {
        kohde.Destroy();

        if (kohde.Tag.ToString() == "vihu")
        {
            _pelaajan1Pisteet.Value += 1;
        }
    }


    /// <summary>
    /// Pelaaja liikkuu animaatiolla.
    /// </summary>
    /// <param name="vektori">vektori</param>
    void LiikutaPelaajaa(Vector vektori)
    {
        _pelaaja1.Move(vektori);
    }


    /// <summary>
    /// Lisätään kentään reuna.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    private void LisaaReuna(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject reuna = PhysicsObject.CreateStaticObject(100, 100);
        reuna.Position = paikka;
        reuna.Color = Color.Gray;
        Add(reuna);
    }


    /// <summary>
    /// Lisätään kenttään taloja.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaBeissi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject talo = PhysicsObject.CreateStaticObject(200, 200);
        talo.Position = paikka;
        talo.Image = _beissi;
        Add(talo);
    }


    /// <summary>
    /// Lisätään kenttään laavaa, esteeksi.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaLaava(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject laacaa = PhysicsObject.CreateStaticObject(90, 90);
        laacaa.Position = paikka;
        laacaa.Image = _laava;
        Add(laacaa);
    }


    /// <summary>
    /// Lisötään kenttään kuihtunut puu.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaKappana(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kappana = PhysicsObject.CreateStaticObject(90, 90);
        kappana.Position = paikka;
        kappana.Image = _musta;
        Add(kappana);
    }


    /// <summary>
    /// Lisätään kenttään tulivuoria.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaTuli(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tulivuori = PhysicsObject.CreateStaticObject(90, 90);
        tulivuori.Position = paikka;
        tulivuori.Image = _tuli;
        Add(tulivuori);
    }


    /// <summary>
    /// Luodaan savuefekti tulivuorille. 
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaSavu(Vector paikka, double leveys, double korkeus)
    {
        Smoke savu = new Smoke();
        savu.Position = paikka;
        Add(savu);
    }


    /// <summary>
    /// Lisätään peliin heladrinksu, josta parantuu.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaHela(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject heladrinksu = PhysicsObject.CreateStaticObject(60, 60);
        heladrinksu.Position = paikka;
        heladrinksu.Tag = "hela";
        heladrinksu.Image = _hela;
        Add(heladrinksu);
    }


    /// <summary>
    /// Lisätään kenttään autoja.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    public void LisaaAuto(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject auto = PhysicsObject.CreateStaticObject(90, 90);
        auto.Position = paikka;
        auto.Image = _pauto;
        Add(auto);
    }


    /// <summary>
    /// Lisätään kenttään kuorma-autoja.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    public void LisaaKauto(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kauto = PhysicsObject.CreateStaticObject(200, 200);
        kauto.Position = paikka;
        kauto.Image = _kAuto;
        Add(kauto);
    }


    /// <summary>
    /// Lisätään kenttään tankkeja.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    public void LisaaTankki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tank = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tank.Position = paikka;
        tank.Image = _tankki;
        Add(tank);
    }


    /// <summary>
    /// Lisätään kenttään betonipossuja.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    public void LisaaBpossu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject bpossut = PhysicsObject.CreateStaticObject(100, 100);
        bpossut.Position = paikka;
        bpossut.Image = _bpossu;
        Add(bpossut);
    }


    /// <summary>
    /// Lisätään puita
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    public void LisaaPuu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject puu = PhysicsObject.CreateStaticObject(leveys, korkeus);
        puu.Position = paikka;
        puu.Image = _puita;
        Add(puu);
    }


    /// <summary>
    /// Luodaan peliin pommi, joka räjähtää, jos zombi tai pelaaja osuu siihen.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">leveys</param>
    /// <param name="korkeus">korkeus</param>
    void LisaaPommi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject pommi = PhysicsObject.CreateStaticObject(leveys, korkeus);
        pommi.Position = paikka;
        pommi.Tag = "pommi";
        pommi.Image = _pommit;
        Add(pommi);
    }


    /// <summary>
    /// Luodaan peliin laskuri, joka laskee pelaajan pisteet.
    /// </summary>
    void LisaaLaskuri()
    {
        _pelaajan1Pisteet = LuoPisteLaskuri(Screen.Left + 100.0, Screen.Top - 50.0);
    }


    /// <summary>
    /// Luodaan peliin pistelaskuri, joka näyttää pelaajan pisteet ruudulla. 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <returns></returns>
    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);
        laskuri.MaxValue = 100;
        Label naytto = new Label(300, 50);
        naytto.BindTo(laskuri);
        naytto.X = x + 10;
        naytto.Y = y + 10;
        naytto.TextColor = Color.Black;
        naytto.BorderColor = Color.Black;
        naytto.Color = Color.LimeGreen;
        naytto.Title = " Zombeja eliminoitu: ";
        Add(naytto);

        return laskuri;
    }


    /// <summary>
    /// Luodaan peliin elämälaskuri, joka näyttää paljonko pelaajalla on helaa.
    /// </summary>
    void LuoElamanLaskuri()
    {
        _elamanlaskuri = new DoubleMeter(10);
        _elamanlaskuri.MaxValue = 10;
        _elamanlaskuri.LowerLimit += ElamaLoppui;
        _elamanlaskuri.AddOverTime(1, 20);
        _elamanlaskuri.LowerLimit += PelaajaKuoli;

        ProgressBar elamapalkki = new ProgressBar(150, 20);
        elamapalkki.X = Screen.Right - 150;
        elamapalkki.Y = Screen.Top - 30;
        elamapalkki.BindTo(_elamanlaskuri);
        Add(elamapalkki);
        elamapalkki.Color = Color.White;
        elamapalkki.BarColor = Color.Red;
        elamapalkki.BorderColor = Color.Black;
    }


    /// <summary>
    /// Kun pelaaja kuolee, palataan alkuvalikkoon.
    /// </summary>
    void PelaajaKuoli()
    {
        ElamaLoppui();
        MultiSelectWindow alkuvalikko =
            new MultiSelectWindow("Pelin alkuvalikko", "Aloita peli", "Parhaat pisteet", "Lopeta");
        Add(alkuvalikko);

        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(1, NaytaParhaatPisteet);
        alkuvalikko.AddItemHandler(2, Exit);
        alkuvalikko.DefaultCancel = 3;
        alkuvalikko.Color = Color.LimeGreen;
        alkuvalikko.SetButtonColor(Color.White);
        alkuvalikko.SetButtonTextColor(Color.Black);
    }


    /// <summary>
    /// pelaaja saa helaa kun se osuu heladrinksuun.
    /// </summary>
    /// <param name="pelaaja1">pelaaja</param>
    /// <param name="kohde">kohde</param>
    void PelaajaParantuu(PhysicsObject pelaaja1, PhysicsObject kohde)
    {
        kohde.Destroy();

        if (kohde.Tag.ToString() == "hela")
        {
            _elamanlaskuri.Value += 1;
        }
    }


    /// <summary>
    /// kun pelaaja törmää zombiin, pelaaja menettää yhden elämän.
    /// </summary>
    /// <param name="tormaaja">tormaaja</param>
    /// <param name="kohde">kohde</param>
    void TormaaViholliseen(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        _elamanlaskuri.Value -= 1;
    }


    /// <summary>
    /// Pelaaja kuolee kun se osuu pommiin.
    /// </summary>
    /// <param name="tormaaja">tormaaja</param>
    /// <param name="kohde">kohde</param>
    void TormaaPommiin(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        kohde.Destroy();

        _elamanlaskuri.Value -= 10;

        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = kohde.Position;
        Add(rajahdys);
    }


    /// <summary>
    /// Kun elämät loppuu pelaaja kuolee ja ei voi liikkua tai ampua enää.
    /// </summary>
    void ElamaLoppui()
    {
        Label tekstikentta = new Label(200, 100, " GAME OVER! ");
        tekstikentta.X = tekstikentta.X + 0;
        tekstikentta.Y = tekstikentta.Y + 300;
        tekstikentta.Color = Color.Black;
        tekstikentta.TextColor = Color.Red;
        tekstikentta.BorderColor = Color.Black;
        Add(tekstikentta);

        _pelaaja1.Animation = new Animation(_pelaajanKuolemaKuva);

        Keyboard.Disable(Key.A);
        Keyboard.Disable(Key.W);
        Keyboard.Disable(Key.S);
        Keyboard.Disable(Key.D);
        Keyboard.Disable(Key.Space);
        Keyboard.Disable(Key.Tab);
    }
}