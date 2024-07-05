using System;
using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;

public class ZeldaClone : PhysicsGame
{
    private const int RUUDUN_KOKO = 40;
    private PlatformCharacter pelaaja;
    private Image pelaajanKuva = LoadImage("survivor.png");

    public override void Begin()
    {
        LuoKentta();
        LuoPelaaja();
        LisaaNappaimet();

        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
    }

    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromStringArray(new string[]
        {
            "####################",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "####################",
        });

        kentta.SetTileMethod('#', LisaaSeina);
        kentta.SetTileMethod('.', LisaaLattia);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.Color = Color.DarkGreen;
    }

    private void LuoPelaaja()
    {
        pelaaja = new PlatformCharacter(40, 40);
        pelaaja.Position = new Vector(0, 0);
        pelaaja.Image = pelaajanKuva;
        pelaaja.CanRotate = false;
        Add(pelaaja);
    }

    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "Liikuta vasemmalle", pelaaja, new Vector(-200, 0));
        Keyboard.Listen(Key.A, ButtonState.Released, Liikuta, "Pysäytä liike", pelaaja, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikuta oikealle", pelaaja, new Vector(200, 0));
        Keyboard.Listen(Key.D, ButtonState.Released, Liikuta, "Pysäytä liike", pelaaja, Vector.Zero);
        Keyboard.Listen(Key.W, ButtonState.Down, Liikuta, "Liikuta ylös", pelaaja, new Vector(0, 200));
        Keyboard.Listen(Key.W, ButtonState.Released, Liikuta, "Pysäytä liike", pelaaja, Vector.Zero);
        Keyboard.Listen(Key.S, ButtonState.Down, Liikuta, "Liikuta alas", pelaaja, new Vector(0, -200));
        Keyboard.Listen(Key.S, ButtonState.Released, Liikuta, "Pysäytä liike", pelaaja, Vector.Zero);
    }

    private void Liikuta(PlatformCharacter hahmo, Vector nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private void LisaaSeina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject seina = PhysicsObject.CreateStaticObject(leveys, korkeus);
        seina.Position = paikka;
        seina.Color = Color.Gray;
        Add(seina);
    }

    private void LisaaLattia(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lattia = PhysicsObject.CreateStaticObject(leveys, korkeus);
        lattia.Position = paikka;
        lattia.Color = Color.DarkGray;
        Add(lattia);
    }
}
