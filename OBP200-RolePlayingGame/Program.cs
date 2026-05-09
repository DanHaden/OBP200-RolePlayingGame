using System.Text;

namespace OBP200_RolePlayingGame
{

    public abstract class Character
    {
        public string Name {get; protected set; }
        public int Hp {get;  protected set; }
        public int MaxHp {get;  protected set; }
        public int Atk {get;  protected  set; }
        public int Def {get;  protected set; }

        public Character(string name, int hp, int maxHp, int atk, int def)
        {
            Name = name;
            Hp = hp;
            MaxHp = maxHp;
            Atk = atk;
            Def = def;
        }

        public virtual void TakeDamage(int damage)
        {
            Hp = Math.Max(0, Hp - Math.Max(0, damage));
        }

        public abstract int CalculateDamage(int enemyDef);

    }

    public class Player : Character
    {   
        public string Cls {get; protected set; }

        public int Gold {get;  protected set; }

        public int XP {get; protected set; }
        
        public int Level {get;  protected set; }
        
        public int Potions {get;  protected set; }
        
        public List<string> Inventory {get; protected set;}

        public Player(string cls, string name, int maxHp, int hp, int atk, int def, int potions, int gold, int xp, int level, List<string> inventory) : base(name, hp, maxHp, atk, def)
        {
            Cls = cls; 
            Gold = gold;
            Potions = potions;
            XP = xp;
            Level = level;
            Inventory = inventory; 
        }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public bool SpendGold(int amount)
        {
            if(Gold < amount) 
            {
                return false;
            }
            else
            {
                Gold -= amount;
                return true;
            }
                
        }

        public void Heal(int amount)
        {
            Hp = Math.Min(MaxHp, Hp + Math.Max(0, amount));
        }
        public void AddPotion(int amount)
        {
            Potions += amount;
        }

        public bool TryUsePotion()
        {
            if (Potions <= 0)
            {
                return false;
            }
            else
            {
                Potions -= 1;
                Heal(12);
                return true;
            }
        }

        public void AddXp(int amount)
        {
            XP += amount;
            CheckIfLevelUp(); 
        }

        private void CheckIfLevelUp()
        {
            int nextThreshold = Level == 1 ? 10 : (Level == 2 ? 25 : (Level == 3 ? 45 : Level * 20));

            if (XP >= nextThreshold)
            {
                Level++;
                switch (Cls)

                {
                    case "Warrior":
                            MaxHp += 6;
                            Atk += 2;
                            Def += 2;
                            break;

                    case "Mage":
                            MaxHp += 4;
                            Atk += 4;
                            Def += 1;
                            break;

                    case "Rogue":
                            MaxHp += 5;
                            Atk += 3;
                            Def += 1;
                            break;

                    default:
                            MaxHp += 4;
                            Atk += 3;
                            Def += 1;
                            break;
                }

                Hp = MaxHp;
                Console.WriteLine($"Du nådde lvl: {Level}! Värden ökade och HP återställd.");

            
            }
                
        }
                
        public void Resting()
        {
            Hp = MaxHp; 
        }

        public void IncreaseAtk(int amount)
        {
            Atk += amount;
        }

        public void IncreaseDef(int amount)
        {
            Def += amount;
        }

        public override void TakeDamage(int damage)
        {
            Hp = Math.Max(0, Hp - Math.Max(0, damage));

        }

        public override int CalculateDamage(int enemyDef)
        {
            int baseDmg = Math.Max(1, Atk - (enemyDef / 2));
            int roll = new Random().Next(0, 3);

            switch (Cls)
            {
                case "Warrior":
                    baseDmg += 1;
                    break;
                case "Mage":
                    baseDmg += 2;
                    break;
                case "Rogue":
                    baseDmg += (new Random().NextDouble() < 0.2) ? 4 : 0;
                    break;
            }

            return Math.Max(1, baseDmg + roll);
        }
            
    }
            
    public class Enemy : Character
    {
    public string Type { get; protected set; }
    public int XpReward { get; protected set; }
    public int GoldReward { get; protected set; }

    public Enemy(string type, string name, int hp, int atk, int def, int xpReward, int goldReward) : base(name, hp, hp, atk, def)
        
        {
            Type = type;
            XpReward = xpReward;
            GoldReward = goldReward;
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            Console.WriteLine($"{Name} tar {damage} skada!");

        }

        public override int CalculateDamage(int playerDef)
        {
            int baseDmg = Math.Max(1, Atk - (playerDef / 2));
            return baseDmg;
        }
    }



    class Program
    {
        static Player playerChar;  

        
        
        static List<string[]> Rooms = new List<string[]>();  
        // Fiendemallar: [type, name, HP, ATK, DEF, XPReward, GoldReward]
        static List<string[]> EnemyTemplates = new List<string[]>();

        // Status för kartan
        static int CurrentRoomIndex = 0;  

        // Random
        static Random Rng = new Random();

        // ======= Main =======

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            InitEnemyTemplates();

            while (true)
            {
                ShowMainMenu();  
                Console.Write("Välj: ");
                var choice = (Console.ReadLine() ?? "").Trim(); 
                if (choice == "1")
                {
                    StartNewGame();
                    RunGameLoop();
                }
                else if (choice == "2")
                {
                    Console.WriteLine("Avslutar...");
                    return;
                }
                else
                {
                    Console.WriteLine("Ogiltigt val.");
                }

                Console.WriteLine();
            }
        }

        // ======= Meny & Init =======

        static void ShowMainMenu()  
        {
            Console.WriteLine("=== Text-RPG ===");
            Console.WriteLine("1. Nytt spel");
            Console.WriteLine("2. Avsluta");
        }

        static void StartNewGame()  
        {
            Console.Write("Ange namn: ");
            var name = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name)) name = "Namnlös"; 

            Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue"); 
            Console.Write("Val: ");
            var clsChoice = (Console.ReadLine() ?? "").Trim();  

            
            List<string> startInventory = new List<string> { "Wooden Sword", "Cloth Armor" };


            switch (clsChoice)
            {
                case "1": 
                    playerChar = new Player("Warrior", name, 40, 40, 7, 5, 2, 15, 0, 1, startInventory); 
                    break;
                case "2": 
                    playerChar = new Player("Mage", name, 28, 28, 10, 2, 2, 15, 0, 1, startInventory); 
                    break;
                case "3": 
                    playerChar = new Player("Rogue", name, 32, 32, 8, 3, 3, 20, 0, 1, startInventory);
                    break;
                default:
                    playerChar = new Player("Warrior", name, 40, 40, 7, 5, 2, 15, 0, 1, startInventory);
                    break;
            }
            // Fyll player-array             //DanHaden detta array skulle eventuellt kunna tillhöra en spelarclass, och den borde nog inkapslas -
             // inventory som semicolon-separerad sträng  //DanHaden eventuellt problem om man vill att "gear" ska tillföra stats. 

            // Initiera karta (linjärt äventyr)
            Rooms.Clear();                              //DanHaden skulle kunna tillhöra en Environment class.
            Rooms.Add(new[] { "battle", "Skogsstig" });
            Rooms.Add(new[] { "treasure", "Gammal kista" });
            Rooms.Add(new[] { "shop", "Vandrande köpman" });
            Rooms.Add(new[] { "battle", "Grottans mynning" });
            Rooms.Add(new[] { "rest", "Lägereld" });
            Rooms.Add(new[] { "battle", "Grottans djup" });
            Rooms.Add(new[] { "boss", "Urdraken" });

            CurrentRoomIndex = 0;   //DanHaden CurrentRoomIndex bestäms flera gånger?

            Console.WriteLine($"Välkommen, {playerChar.Name} the {playerChar.Cls}!");
            
        }

        static void RunGameLoop()
        {
            while (true)
            {
                var room = Rooms[CurrentRoomIndex]; 
                Console.WriteLine($"--- Rum {CurrentRoomIndex + 1}/{Rooms.Count}: {room[1]} ({room[0]}) ---"); 

                bool continueAdventure = EnterRoom(room[0]);
                
                if (IsPlayerDead())  
                {
                    Console.WriteLine("Du har stupat... Spelet över.");
                    break;
                }
                
                if (!continueAdventure)
                {
                    Console.WriteLine("Du lämnar äventyret för nu.");
                    break;
                }

                CurrentRoomIndex++;
                
                if (CurrentRoomIndex >= Rooms.Count)
                {
                    Console.WriteLine();
                    Console.WriteLine("Du har klarat äventyret!");
                    break;
                }
                
                Console.WriteLine();
                Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
                Console.Write("Val: ");
                var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

                if (post == "Q")
                {
                    Console.WriteLine("Tillbaka till huvudmenyn.");
                    break;
                }

                Console.WriteLine();
            }
        }

        // ======= Rumshantering =======

        static bool EnterRoom(string type)   
        {
            switch ((type ?? "battle").Trim()) 
            {
                case "battle":
                    return DoBattle(isBoss: false);
                case "boss":
                    return DoBattle(isBoss: true);
                case "treasure":
                    return DoTreasure();
                case "shop":
                    return DoShop();
                case "rest":
                    return DoRest();
                default:
                    Console.WriteLine("Du vandrar vidare...");
                    return true;
            }
        }

        // ======= Strid =======

        static bool DoBattle(bool isBoss)   
        {
            Enemy enemy = GenerateEnemy(isBoss);
            Console.WriteLine($"En {enemy.Name} dyker upp! (HP {enemy.Hp}, ATK {enemy.Atk}, DEF {enemy.Def})");

            int enemyHp = enemy.Hp; 
            int enemyAtk = enemy.Atk;
            int enemyDef = enemy.Def;

            while (enemyHp > 0 && !IsPlayerDead())
            {
                Console.WriteLine();
                ShowStatus();
                Console.WriteLine($"Fiende: {enemy.Name} HP={enemyHp}");
                Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
                if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
                Console.Write("Val: ");

                var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

                if (cmd == "A")
                {
                    int damage = playerChar.CalculateDamage(enemyDef);
                    enemyHp -= damage; //därä
                    Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
                }
                else if (cmd == "X")
                {
                    int special = UseClassSpecial(enemyDef, isBoss);
                    enemyHp -= special;
                    Console.WriteLine($"Special! {enemy.Name} tar {special} skada.");
                }
                else if (cmd == "P")
                {
                    UsePotion();
                }
                else if (cmd == "R" && !isBoss)
                {
                    if (TryRunAway())
                    {
                        Console.WriteLine("Du flydde!");
                        return true; // fortsätt äventyr
                    }
                    else
                    {
                        Console.WriteLine("Misslyckad flykt!");
                    }
                }
                else
                {
                    Console.WriteLine("Du tvekar...");
                }

                if (enemyHp <= 0) break;

                // Fiendens tur
                int enemyDamage = enemy.CalculateDamage(playerChar.Def);
                playerChar.TakeDamage(enemyDamage);
                Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
            }

            if (IsPlayerDead())
            {
                return false; // avsluta äventyr
            }

            // Vinstrapporter, XP, guld, loot
            int xpReward = enemy.XpReward;
            int goldReward = enemy.GoldReward;

            AddPlayerXp(xpReward);
            AddPlayerGold(goldReward);

            Console.WriteLine($"Seger! +{xpReward} XP, +{goldReward} guld.");
            MaybeDropLoot(enemy.Name);

            return true;
        }

        static Enemy GenerateEnemy(bool isBoss)
        {
            if (isBoss)      
            {
                // Boss-mall
                return new Enemy("boss", "Urdraken", 55, 9, 4, 30, 50);
                    
            }
            else
            {
                // Slumpa bland templates
                var template = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];
                
                // Slmumpmässig justering av stats
                int hp = ParseInt(template[2], 10) + Rng.Next(-1, 3);
                int atk = ParseInt(template[3], 3) + Rng.Next(0, 2);
                int def = ParseInt(template[4], 0) + Rng.Next(0, 2);
                int xp = ParseInt(template[5], 4) + Rng.Next(0, 3);
                int gold = ParseInt(template[6], 2) + Rng.Next(0, 3);
                return new Enemy( template[0], template[1], hp, atk, def, xp, gold);
                                    
            }
        }

        static void InitEnemyTemplates()
        {
            EnemyTemplates.Clear();
            EnemyTemplates.Add(new[] { "beast", "Vildsvin", "18", "4", "1", "6", "4" });
            EnemyTemplates.Add(new[] { "undead", "Skelett", "20", "5", "2", "7", "5" });
            EnemyTemplates.Add(new[] { "bandit", "Bandit", "16", "6", "1", "8", "6" });
            EnemyTemplates.Add(new[] { "slime", "Geléslem", "14", "3", "0", "5", "3" });
        }


        static int UseClassSpecial(int enemyDef, bool vsBoss)
        {
            string cls = playerChar.Cls ?? "Warrior";
            int specialDmg;

            // Hantering av specialförmågor
            if (cls == "Warrior")
            {
                // Heavy Strike: hög skada men självskada
                Console.WriteLine("Warrior använder Heavy Strike!");
                int atk = playerChar.Atk;
                specialDmg = Math.Max(2, atk + 3 - enemyDef);
                playerChar.TakeDamage(2);
            }
            else if (cls == "Mage")
            {
                // Fireball: stor skada, kostar guld
                int gold = playerChar.Gold;
                if (gold >= 3)
                {
                    Console.WriteLine("Mage kastar Fireball!");
                    playerChar.SpendGold(-3);
                    int atk = playerChar.Atk;
                    specialDmg = Math.Max(3, atk + 5 - (enemyDef / 2));
                }
                else
                {
                    Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                    specialDmg = 0;
                }
            }
            else if (cls == "Rogue")
            {
                // Backstab: chans att ignorera försvar, hög risk/hög belöning
                if (Rng.NextDouble() < 0.5)
                {
                    Console.WriteLine("Rogue utför en lyckad Backstab!");
                    int atk = playerChar.Atk;
                    specialDmg = Math.Max(4, atk + 6);
                }
                else
                {
                    Console.WriteLine("Backstab misslyckades!");
                    specialDmg = 1;
                }
            }
            else
            {
                specialDmg = 0;
            }

            // Dämpa skada mot bossen
            if (vsBoss)
            {
                specialDmg = (int)Math.Round(specialDmg * 0.8);
            }

            return Math.Max(0, specialDmg);
        }


        static void UsePotion()
        {
            if (playerChar.TryUsePotion())
            {
                Console.WriteLine($"Du dricker en potion och återfår {12} HP");
            }
            else
            {
                Console.WriteLine("Du har inga potions kvar"); 
            }
        }

        static bool TryRunAway()
        {
            // Flyktschans baserad på karaktärsklass
            string cls = playerChar.Cls ?? "Warrior";
            double chance = 0.25;
            if (cls == "Rogue") chance = 0.5;
            if (cls == "Mage") chance = 0.35;
            return Rng.NextDouble() < chance;
        }

        static bool IsPlayerDead()
        {
            return playerChar.Hp <= 0;
        }

        static void AddPlayerXp(int amount)
        {
            playerChar.AddXp(amount); 
        }

        static void AddPlayerGold(int amount)
        {
            playerChar.AddGold(amount); 
        }

        

        static void MaybeDropLoot(string enemyName)
        {
            // Enkel loot-regel
            if (Rng.NextDouble() < 0.35)
            {
                string item = "Minor Gem";
                if (enemyName.Contains("Urdraken")) item = "Dragon Scale";

                playerChar.Inventory.Add(item);

                Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
            }
        }

        // ======= Rumshändelser =======

        static bool DoTreasure()
        {
            Console.WriteLine("Du hittar en gammal kista...");
            if (Rng.NextDouble() < 0.5)
            {
                int gold = Rng.Next(8, 15);
                AddPlayerGold(gold);
                Console.WriteLine($"Kistan innehåller {gold} guld!");
            }
            else
            {
                var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
                string found = items[Rng.Next(items.Length)];
                playerChar.Inventory.Add(found);
                Console.WriteLine($"Du plockar upp: {found}");
            }
            return true;
        }

        static bool DoShop()
        {
            Console.WriteLine("En vandrande köpman erbjuder sina varor:");
            while (true)
            {
                Console.WriteLine($"Guld: {playerChar.Gold} | Drycker: {playerChar.Potions}");
                Console.WriteLine("1) Köp dryck (10 guld)");
                Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
                Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
                Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
                Console.WriteLine("5) Lämna butiken");
                Console.Write("Val: ");
                var val = (Console.ReadLine() ?? "").Trim();

                if (val == "1")
                {
                    TryBuy(10, () => playerChar.AddPotion(1), "Du köpte en dryck!"); 
                    
                }
                else if (val == "2")
                {
                    TryBuy(25, () => playerChar.IncreaseAtk(2), "Du köper ett bättre vapen.");
                }
                else if (val == "3")
                {
                    TryBuy(25, () => playerChar.IncreaseDef(2), "Du köper en bättre rustning.");
                }
                else if (val == "4")
                {
                    SellMinorGems();
                }
                else if (val == "5")
                {
                    Console.WriteLine("Du säger adjö till köpmannen.");
                    break;
                }
                else
                {
                    Console.WriteLine("Köpmannen förstår inte ditt val.");
                }
            }
            return true;
        }

        static void TryBuy(int cost, Action apply, string successMsg)
        {
            
            if (playerChar.SpendGold(cost))
            {
                apply();
                Console.WriteLine(successMsg);
                
            }
            else
            {
                Console.WriteLine("Du har inte råd.");
            }
        }

        static void SellMinorGems()
        {
            
            if (playerChar.Inventory == null || playerChar.Inventory.Count == 0)
            {
                Console.WriteLine("Du har inga föremål att sälja.");
                return;
            }

            
            int count = playerChar.Inventory.Count(x => x == "Minor Gem");
            if (count == 0)
            {
                Console.WriteLine("Inga 'Minor Gem' i väskan.");
                return;
            }

            playerChar.Inventory.RemoveAll(x => x == "Minor Gem");

            AddPlayerGold(count * 5);
            Console.WriteLine($"Du säljer {count} st Minor Gem för {count * 5} guld.");
        }

        static bool DoRest()
        {
            Console.WriteLine("Du slår läger och vilar.");
            playerChar.Resting(); 
            Console.WriteLine("HP återställt till max.");
            return true;
            
        }

        // ======= Status =======

        static void ShowStatus()  
        {
            Console.WriteLine($"[{playerChar.Name} | {playerChar.Cls}]  HP {playerChar.Hp}/{playerChar.MaxHp}  ATK {playerChar.Atk}  DEF {playerChar.Def}  LVL {playerChar.Level}  XP {playerChar.XP}  Guld {playerChar.Gold}  Drycker {playerChar.Potions}");
            
            if (playerChar.Inventory.Count > 0)
            {
                string contents = string.Join(", ", playerChar.Inventory); 
                Console.WriteLine($"Väska: {contents}");
            }
        }
        
        // ======= Hjälpmetoder =======

        static int ParseInt(string s, int fallback)
        {
            try
            {
                int value = Convert.ToInt32(s);
                return value;
            }
            catch (Exception e)
            {   
                Console.WriteLine(e);
                return fallback;
            }
        }
    }
}