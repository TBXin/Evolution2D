using System;
using System.Collections.Generic;

namespace Evolution.Core
{
	public class World
	{
		private static readonly Random s_rnd = new Random();
		private readonly Entity[,] m_entitiesMap;
		public List<Creature> LiveCreatures = new List<Creature>();

		public World(int width, int height)
		{
			Width = width;
			Height = height;

			m_entitiesMap = new Entity[Width, Height];
		}

		public int Width { get; }

		public int Height { get; }

		public Entity this[int x, int y]
		{
			get
			{
				if (x < 0 || x > Width - 1) throw new ArgumentOutOfRangeException(nameof(x));
				if (y < 0 || y > Height - 1) throw new ArgumentOutOfRangeException(nameof(y));

				return m_entitiesMap[x, y];
			}
			set
			{
				if (value != null)
				{
					value.X = x;
					value.Y = y;
				}
				m_entitiesMap[x, y] = value;
			}
		}

		public void Iteration()
		{
			foreach (var entity in GetFlatEntitiesCollection())
			{
				entity.Process(this);
			}
		}

		public EntityType GetEntityTypeAt(int x, int y)
		{
			if (x < 0 || x > Width - 1 || y < 0 || y > Height - 1) return EntityType.Wall;

			var entity = this[x, y];
			return entity?.EntityType ?? EntityType.Empty;
		}

		public EntityType GetEntityTypeAt(Point point)
		{
			return GetEntityTypeAt(point.X, point.Y);
		}

		public void AddEntity(Entity entity, int x, int y)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (entity is Creature creature)
			{
				LiveCreatures.Add(creature);
			}

			entity.X = x;
			entity.Y = y;
			this[x, y] = entity;
		}


		public void MoveEntity(Entity entity, Point point)
		{
			MoveEntity(entity, point.X, point.Y);
		}

		public void MoveEntity(Entity entity, int x, int y)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			this[entity.X, entity.Y] = null;
			entity.X = x;
			entity.Y = y;

			var e = this[entity.X, entity.Y];
			if (e != null)
			{
				System.Diagnostics.Debugger.Break();
			}

			this[entity.X, entity.Y] = entity;
		}

		public void RemoveEntity(Point location)
		{
			RemoveEntity(location.X, location.Y);
		}

		public void RemoveEntity(int x, int y)
		{
			var entity = this[x, y];
			if (entity == null) throw new InvalidOperationException();

			if (entity is Creature creature && creature.Health <= 0)
			{
				LiveCreatures.Remove(creature);
			}

			m_entitiesMap[x, y] = null;
		}

		public void PrepareWorld(Creature[] creatures = null)
		{
			for (var x = 0; x < Width; x++)
			{
				for (var y = 0; y < Height; y++)
				{
					var entity = m_entitiesMap[x, y];
					if (entity == null || entity.EntityType == EntityType.Wall) continue;

					RemoveEntity(x, y);
				}
			}

			var count = creatures?.Length ?? 64;

			for (var i = 0; i < count; i++)
			{
				var iLocal = i;
				SpawnEntity(() => creatures == null ? new Creature() : creatures[iLocal]);
			}

			for (var i = 0; i < 60; i++)
			{
				SpawnEntity(() => new Food(false));
				SpawnEntity(() => new Food(false));
			}
		}

		private void SpawnEntity(Func<Entity> entityFactory)
		{
			while (true)
			{
				var x = s_rnd.Next(0, Width);
				var y = s_rnd.Next(0, Height);

				if (this[x, y] != null) continue;

				AddEntity(entityFactory(), x, y);
				return;
			}
		}

		public void AddFoodOrPoison()
		{
			SpawnEntity(() => new Food(s_rnd.Next(0, 2) == 1));
		}

		public IEnumerable<Entity> GetFlatEntitiesCollection()
		{
			for (var i = 0; i < Width; i++)
			{
				for (var j = 0; j < Height; j++)
				{
					var entity = this[i, j];
					if (entity != null) yield return entity;
				}
			}
		}
	}
}
