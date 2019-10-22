using System;
using System.Collections.ObjectModel;
using static System.Console;

namespace Coding.Exercise
{
    public abstract class Creature
    {
        public int Attack { get; set; }
        public int Defense { get; set; }

        public override string ToString()
        {
            return $"[Creature] {{\n" +
                $"\tCreature Type : {GetType().Name},\n" +
                $"\t{nameof(Attack)} : {Attack},\n" +
                $"\t{nameof(Defense)} : {Defense}\n" +
                $"}}";
        }
    }

    public class Goblin : Creature
    {
        public Goblin(Game game) {
            Attack = 1;
            Defense = 1;
        }
    }

    public class GoblinKing : Goblin
    {
        public static bool AlreadyPlaced;

        public GoblinKing(Game game) : base(game) {
            if (AlreadyPlaced) throw new InvalidOperationException("There can be only one GoblinKing!");

            Attack = 3;
            Defense = 3;
            AlreadyPlaced = true;
        }
    }

    public class CreatureModifier
    {
        protected Creature creature;
        protected CreatureModifier next;

        public CreatureModifier(Creature creature)
        {
            if (creature == null) throw new ArgumentNullException(paramName: nameof(creature));

            this.creature = creature;
        }

        public void Add(CreatureModifier cm)
        {
            if (next != null) next.Add(cm);
            else next = cm;
        }

        public virtual void Handle() => next?.Handle();
    }

    public class GoblinKingAuraModifier : CreatureModifier {
        public GoblinKingAuraModifier(Creature creature) : base(creature) {}
        
        public override void Handle() {
            if (creature is GoblinKing) base.Handle();

            creature.Attack += 1;
            base.Handle();
        }
    }

    public class StrengthInNumbersModifier : CreatureModifier {
        public static int NUMBERS;

        public StrengthInNumbersModifier(Creature creature) : base(creature) {}
        
        public override void Handle() {            
            creature.Attack = NUMBERS == 0 ? 1 : NUMBERS;
        }
    }

    public class Game
    {
        public ObservableCollection<Creature> Creatures = new ObservableCollection<Creature>();
        
        public Game() {
            Creatures.CollectionChanged += Creatures_CollectionChanged;
        }

        private void Creatures_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            GoblinKing.AlreadyPlaced = false;
            StrengthInNumbersModifier.NUMBERS = Creatures.Count; // All creatures are Goblins!

            foreach(Creature creature in Creatures) {
                CreatureModifier masterList = new CreatureModifier(creature);
                if (GoblinKing.AlreadyPlaced) masterList.Add(new GoblinKingAuraModifier(creature));
                if (Creatures.Count > 1) masterList.Add(new StrengthInNumbersModifier(creature));
                masterList.Handle();
                WriteLine(creature);
            }
        }
    }
}
