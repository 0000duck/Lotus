﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lotus.ECS {
    public sealed class Entity {

        static int nextEntityId;

        public static int Allocate() {
            return nextEntityId++;
        }

        public static Entity Wrap(int id) {
            Entity ent;
            if (!IdMap<Entity>.Map.TryGetValue(id, out ent)) {
                ent = new Entity(id);
            }
            return ent;
        }

        public static T Get<T>(int id) where T : Aspect {
            T t;
            IdMap<T>.Map.TryGetValue(id, out t);
            return t;
        }

        public static bool Has<T>(int id) where T : Aspect {
            return IdMap<T>.Map.ContainsKey(id);
        }

        public static T Add<T>(int id) where T : Aspect {
            T t = (T)Activator.CreateInstance(typeof(T), id);
            IdMap<T>.Map.Add(id, t);
            foreach (Module module in Engine.Modules) module.Reveille(t);
            return t;
        }

        public static bool Remove<T>(int id) where T : Aspect {
            foreach (Module module in Engine.Modules) module.Taps(IdMap<T>.Map[id]);
            return IdMap<T>.Map.Remove(id);
        }

        public readonly int Id;

        private Entity(int id) {
            Id = id;
        }

        public Entity() {
            Id = Allocate();
        }

        public T Get<T>() where T : Aspect {
            return Get<T>(Id);
        }

        public bool Has<T>() where T : Aspect {
            return Has<T>(Id);
        }

        public T Add<T>() where T : Aspect {
            return Add<T>(Id);
        }
        public bool Remove<T>() where T : Aspect {
            return Remove<T>(Id);
        }
    }
}
