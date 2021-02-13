using KarmanNet.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZooExample.Internal;

namespace ZooExample {

    public class ZooExample : MonoBehaviour {
        [ContextMenu("Run")]
        private void Run() {
            Zoo zoo = new Zoo();

            byte[] tigerEnclosure = zoo.enclosureFactory.GetBytes(new TigerEnclosure(55));
            zoo.AddEnclosure(tigerEnclosure);

            byte[] tiger1 = zoo.animalFactory.GetBytes(new Tiger(50));
            byte[] tiger2 = zoo.animalFactory.GetBytes(new Tiger(60));
            byte[] whale1 = zoo.animalFactory.GetBytes(new Whale(670.4f));
            byte[] whale2 = zoo.animalFactory.GetBytes(new Whale(915.7f));
            byte[] alien = zoo.animalFactory.GetBytes(new Alien("mars"));

            zoo.AddAnimal(tiger1);
            zoo.AddAnimal(tiger2);
            zoo.AddAnimal(whale1);
            zoo.AddAnimal(whale2);
            zoo.AddAnimal(alien);
        }
    }

    public class Alien : Animal {
        private readonly string origin;

        public Alien(byte[] bytes) : base(bytes) {
            origin = ReadString();
        }

        public Alien(string origin) : base(Bytes.Of(origin)) {
            this.origin = origin;
        }

        public string GetOrigin() {
            return origin;
        }

        public override bool IsValid() {
            return origin != null && origin.Length > 0;
        }
    }

    public class Tiger : Animal {
        private readonly int strength;

        public Tiger(byte[] bytes) : base(bytes) {
            strength = ReadInt();
        }

        public Tiger(int strength) : base(Bytes.Of(strength)) {
            this.strength = strength;
        }

        public int GetStrength() {
            return strength;
        }

        public override bool IsValid() {
            return strength > 0;
        }
    }

    public class Whale : Animal {
        private readonly float volume;

        public Whale(byte[] bytes) : base(bytes) {
            volume = ReadFloat();
        }

        public Whale(float volume) : base(Bytes.Of(volume)) {
            this.volume = volume;
        }

        public float GetVolume() {
            return volume;
        }

        public override bool IsValid() {
            return volume > 0;
        }
    }

    public class TigerEnclosure : Enclosure<Tiger> {
        private readonly int maxStrength;

        public TigerEnclosure(byte[] bytes) : base(bytes) {
            maxStrength = ReadInt();
        }

        public TigerEnclosure(int maxStrength): base(Bytes.Of(maxStrength)) {
            this.maxStrength = maxStrength;
        }

        public static TigerEnclosure Identity() {
            return new TigerEnclosure(30);
        }

        protected override bool Enclose(Tiger tiger) {
            StringBuilder message = new StringBuilder();
            message.Append($"A tiger is being inspected before being added to an enclosure! The tiger has a strength of '{tiger.GetStrength()}', which is ");
            if (tiger.GetStrength() <= maxStrength) {
                Debug.Log(message.Append("perfectly fine for this enclosure. The tiger is added to the enclosure."));
                return true;
            } else {
                Debug.LogWarning(message.Append("too strong for this enclosure. The tiger is not added to the enclosure."));
                return false;
            }
        }

        public override bool IsValid() {
            return maxStrength > 0;
        }
    }

    public class WhaleEnclosure : Enclosure<Whale> {
        private readonly float maxVolume;

        public WhaleEnclosure(byte[] bytes) : base(bytes) {
            maxVolume = ReadFloat();
        }

        public WhaleEnclosure(float maxVolume) : base(Bytes.Of(maxVolume)) {
            this.maxVolume = maxVolume;
        }

        public static WhaleEnclosure Identity() {
            return new WhaleEnclosure(899.99f);
        }

        protected override bool Enclose(Whale whale) {
            StringBuilder message = new StringBuilder();
            message.Append($"A whale is being inspected before being added to an enclosure! The whale has a volume of '{whale.GetVolume()}', which is ");
            if (whale.GetVolume() <= maxVolume) {
                Debug.Log(message.Append("perfectly fine for this enclosure. The whale is added to the enclosure."));
                return true;
            } else {
                Debug.LogWarning(message.Append("too large for this enclosure. The whale is not added to the enclosure."));
                return false;
            }
        }

        public override bool IsValid() {
            return maxVolume > 0;
        }
    }
}

namespace ZooExample.Internal {
    public abstract class Animal : ByteConstructable {
        public Animal(byte[] bytes) : base(bytes) { }
    }

    public abstract class Enclosure : ByteConstructable {
        public Enclosure(byte[] bytes) : base(bytes) {
        }

        public abstract bool CanEnclose(Animal animal);
        public abstract bool Enclose(Animal animal);
    }

    public abstract class Enclosure<T> : Enclosure where T : Animal {
        protected Enclosure(byte[] bytes) : base(bytes) {
        }

        public override bool CanEnclose(Animal animal) {
            return animal is T;
        }

        public override bool Enclose(Animal animal) {
            if (!(animal is T t)) {
                throw new Exception($"{GetType().Name} can not enclose a {animal.GetType().Name}");
            }
            return Enclose(t);
        }

        protected abstract bool Enclose(T animal);
    }

    public class Zoo {
        public readonly Factory<Animal> animalFactory = Factory<Animal>.BuildFromAllAssemblies();
        public readonly Factory<Enclosure> enclosureFactory = Factory<Enclosure>.BuildFromAllAssemblies();

        private readonly List<object> animals = new List<object>();
        private readonly Dictionary<Type, Enclosure> enclosures = new Dictionary<Type, Enclosure>();

        private Enclosure BuildEnclosureFor(Type animalType) {
            Type enclosureType = typeof(Enclosure<>).MakeGenericType(animalType);
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())) {
                if (enclosureType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract) {
                    Debug.Log($"Zoo is building a(n) {type.Name}, to be able to enclose an animal of type {animalType.Name}.");
                    return (Enclosure)type.GetMethod("Identity").Invoke(null, null);
                }
            }
            Debug.LogWarning($"Cannot build an enclosure for a(n) {animalType.Name}, because an enclosure for that type of animal does not exist.");
            return null;
        }

        public bool AddAnimal(byte[] bytes) {
            Animal animal = animalFactory.FromBytes(bytes);
            Type animalType = animal.GetType();
            if (!enclosures.TryGetValue(animalType, out Enclosure enclosure)) {
                enclosure = BuildEnclosureFor(animalType);
                if (enclosure == null) {
                    return false;
                }
                enclosures.Add(animalType, enclosure);
            }
            bool hasPassedInspection = enclosure.Enclose(animal);
            if (hasPassedInspection) {
                animals.Add(animal);
            }
            return hasPassedInspection;
        }

        public bool AddEnclosure(byte[] bytes) {
            Enclosure enclosure = enclosureFactory.FromBytes(bytes);
            Type enclosureBaseType = enclosure.GetType();
            while (!enclosureBaseType.IsGenericType || enclosureBaseType.GetGenericTypeDefinition() != typeof(Enclosure<>)) {
                enclosureBaseType = enclosureBaseType.BaseType;
                if (enclosureBaseType == typeof(object)) {
                    return false;
                }
            }
            Type animalType = enclosure.GetType().BaseType.GetGenericArguments()[0];
            Debug.Log($"Adding a {enclosure.GetType().Name} that can enclose animals of type {animalType.Name}.");
            enclosures.Add(animalType, enclosure);
            return true;
        }
    }
}

/*
 * In C# Source Generator (https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/)
 * [Packable, Identity]
 * public class WhaleEnclosure : Enclosure<Whale> {
 *   [Getter, IdentityValue(899.99f), PositiveOrZero, LessThan(500)]
 *   private readonly float maxVolume;
 *   ...
 * }
 * 
 * In JSON
 * { "name": "WhaleEnclosure", base: "Enclosure<Whale>", fields: [{"name": "maxVolume", "type": "float", "identity": 899.99, "evaluations": [">= 0", "<= 500"] }, ...] }
 * 
 * In DSL
 * WhaleEnclosure : Enclosure<Whale> (
 *   [float, maxVolume, 899.99f, > 0]
 *   ...
 * )
 * 
 * In Visual Studio Code Snippet
 * public class $name$ : $base$ {
 *   private readonly $typeA$ $nameA$;
 *   
 *   public $name$(byte[] bytes): base(bytes) {
 *     $nameA$ = Read$typeA$;
 *   }
 *   
 *   public $name$($typaA$ $nameA$): base(Bytes.Of($nameA$)) {
 *     this.$nameA$ = $nameA$;
 *   }
 *   
 *   public static $name$ Identity() {
 *     return new $name$($identityA$);
 *   }
 *   
 *   public override bool IsValid() {
 *     return $nameA$ $evaluationA$;
 *   }
 * }
 * 
 * public class $name$ : $base$ {
 *   private readonly $typeA$ $nameA$;
 *   private readonly $typeB$ $nameB$;
 *   
 *   public $name$(byte[] bytes): base(bytes) {
 *     $nameA$ = Read$typeA$;
 *     $nameB$ = Read$typeB$;
 *   }
 *   
 *   public $name$($typaA$ $nameA$, $typeB$ $nameB$): base(
 *     Bytes.Pack(Bytes.Of($nameA$), Bytes.Of($nameB))
 *   ) {
 *     this.$nameA$ = $nameA$;
 *     this.$nameB$ = $nameB$;
 *   }
 *   
 *   public static $name$ Identity() {
 *     return new $name$($identityA$, $identityB$);
 *   }
 *   
 *   public override bool IsValid() {
 *     return $nameA$ $evaluationA$ && $nameB$ $evaluationB$;
 *   }
 *   
 *   public $typeA$ Get$nameA$() {
 *     return $nameA$;
 *   }
 *   
 *   public $typeB$ Get$nameB$() {
 *     return $nameB$;
 *   }
 * }
 */