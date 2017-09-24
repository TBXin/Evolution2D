using System;

namespace Evolution.Core
{
	public abstract class Entity
	{
		public int X { get; set; }

		public int Y { get; set; }

		public abstract EntityType EntityType { get; }

		public virtual void Process(World world)
		{
		}
	}

	public class Wall : Entity
	{
		public override EntityType EntityType => EntityType.Wall;
	}

	public class Food : Entity
	{
		private bool m_isPoisoned;

		public Food(bool isPoisoned)
		{
			m_isPoisoned = isPoisoned;
		}

		public void RemovePoison()
		{
			m_isPoisoned = false;
		}

		public override EntityType EntityType => m_isPoisoned ? EntityType.Poison : EntityType.Food;
	}

	public class Creature : Entity
	{
		private static readonly Random s_rnd = new Random();
		private readonly int[] m_instructions = new int[64];
		private int m_instructionIndex;
		private int m_health;

		public Creature(int[] genome = null)
		{
			Health = 35;

			SightVector = s_rnd.Next(0, 8 + 1);

			if (genome == null)
			{
				for (var i = 0; i < m_instructions.Length; i++)
				{
					m_instructions[i] = s_rnd.Next(0, m_instructions.Length);
				}
			}
			else
			{
				m_instructions = genome;
			}
		}

		public int[] CopyGenome()
		{
			var result = new int[m_instructions.Length];
			for (var i = 0; i < m_instructions.Length; i++)
			{
				result[i] = m_instructions[i];
			}
			return result;
		}

		public void Mutate()
		{
			var genesToChange = /*s_rnd.Next(1, 2)*/1;

			for (var i = 0; i < genesToChange; i++)
			{
				var geneIdx = s_rnd.Next(0, m_instructions.Length);
				m_instructions[geneIdx] = s_rnd.Next(0, m_instructions.Length);
			}
		}

		public int SightVector { get; set; }

		public int Health
		{
			get => m_health;
			set
			{
				m_health = value;
				if (m_health > 90) m_health = 90;
			}
		}

		public override EntityType EntityType => EntityType.Creature;

		public override void Process(World world)
		{
			for (var i = 0; i < 10; i++)
			{
				var instruction = m_instructions[m_instructionIndex];

				// Move
				if (instruction < 8)
				{
					Move(instruction, world);
					break;
				}
				// Look
				if (instruction < 16)
				{
					var location = GetPointFromVector(instruction);
					var entityType = world.GetEntityTypeAt(location);

					SetNextInstructionIndex((int)entityType + 1);
				}
				// Rotate
				else if (instruction < 24)
				{
					SightVector = (SightVector + instruction) % 8;
					SetNextInstructionIndex(1);
				}
				// Eat & Depoison
				else if (instruction < 32)
				{
					Grab(instruction, world);
					break;
				}
				// Jump to command
				else
				{
					SetNextInstructionIndex(instruction);
				}
			}

			Health--;
			if (Health <= 0)
			{
				world.RemoveEntity(X, Y);
			}
		}

		private void SetNextInstructionIndex(int offset)
		{
			m_instructionIndex = (m_instructionIndex + offset) % m_instructions.Length;
		}

		private void Move(int directionVector, World world)
		{
			// +---+---+---+
			// | 0 | 1 | 2 |
			// +---+---+---+
			// | 7 | X | 3 |
			// +---+---+---+
			// | 6 | 5 | 4 |
			// +---+---+---+

			var location = GetPointFromVector(directionVector);
			var entityType = world.GetEntityTypeAt(location.X, location.Y);

			if (entityType == EntityType.Empty)
			{
				world.MoveEntity(this, location);
			}
			else if (entityType == EntityType.Food)
			{
				world.RemoveEntity(location);
				world.MoveEntity(this, location);
				Health += 10;
				world.AddFoodOrPoison();
			}
			else if (entityType == EntityType.Poison)
			{
				Health = 0;
				world.RemoveEntity(X, Y);
				world.AddEntity(new Food(true), X, Y);
			}

			SetNextInstructionIndex((int)entityType + 1);
		}

		private void Grab(int directionVector, World world)
		{
			var location = GetPointFromVector(directionVector);
			var entityType = world.GetEntityTypeAt(location);

			if (entityType == EntityType.Food)
			{
				world.RemoveEntity(location.X, location.Y);
				Health += 10;
				world.AddFoodOrPoison();
			}
			if (entityType == EntityType.Poison)
			{
				world[location.X, location.Y] = new Food(false);
			}

			SetNextInstructionIndex((int)entityType + 1);
		}

		private Point GetPointFromVector(int initialVector)
		{
			var vector = (initialVector + SightVector) % 8;

			var newX = X;
			var newY = Y;

			if (vector == 0 || vector == 1 || vector == 2) newY--;
			if (vector == 2 || vector == 3 || vector == 4) newX++;

			if (vector == 4 || vector == 5 || vector == 6) newY++;
			if (vector == 6 || vector == 7 || vector == 0) newX--;

			return new Point(newX, newY);
		}
	}

	public enum EntityType
	{
		Empty = 0,
		Wall = 1,
		Food = 2,
		Poison = 3,
		Creature = 4
	}
}
