using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Evolution.Core;

namespace Evolution.Visualizer.Controls
{
	public class EvolutionCanvas : Control
	{
		private const int BlockSize = 21;

		public EvolutionCanvas()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
		}

		public World World { get; set; }

		private readonly Pen m_gridPen = new Pen(Color.FromArgb(240, 240, 240));
		private readonly Pen m_arrowPen = new Pen(Color.Black, 4) { StartCap = LineCap.ArrowAnchor };
		private readonly SolidBrush m_creatureBrush = new SolidBrush(Color.FromArgb(0x23, 0xAE, 0xEA));
		private readonly SolidBrush m_foodBrush = new SolidBrush(Color.FromArgb(0x9B,0xC5,0x3D));
		private readonly SolidBrush m_poisonBrush = new SolidBrush(Color.FromArgb(0xFA,0x8D,0x29));
		private readonly SolidBrush m_wallBrush = new SolidBrush(Color.FromArgb(0xC9, 0xCA, 0xCB));

		private readonly Font m_creatureFont = new Font("Courier New", 8.25F, FontStyle.Bold);
		private readonly StringFormat m_createStringFormat = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

		private void DrawGrid(Graphics gfx, int fieldWidth, int fieldHeight)
		{
			for (var mx = 1; mx < World.Width - 1; mx++)
			{
				gfx.DrawLine(m_gridPen, mx * BlockSize, 0, mx * BlockSize, fieldHeight);
			}

			for (var my = 1; my < World.Height - 1; my++)
			{
				gfx.DrawLine(m_gridPen, 0, my * BlockSize, fieldWidth, my * BlockSize);
			}

			gfx.DrawRectangle(Pens.Black, 0, 0, fieldWidth, fieldHeight);
		}

		private void DrawEntities(Graphics gfx)
		{
			for (var mx = 0; mx < World.Width; mx++)
			{
				for (var my = 0; my < World.Height; my++)
				{
					var entityRect = new Rectangle(mx * BlockSize, my * BlockSize, BlockSize, BlockSize);
					var entity = World[mx, my];

					if (entity == null || entity.EntityType == EntityType.Empty) continue;

					var entityFillRect = new Rectangle(entityRect.X + 1, entityRect.Y + 1, entityRect.Width - 1, entityRect.Height - 1);
					var fillBrush = GetEntityBrush(entity.EntityType);

					if (entity is Creature creature)
					{
						var creatureCenterX = entityFillRect.X + entityFillRect.Width / 2f;
						var creatureCenterY = entityFillRect.Y + entityFillRect.Height / 2f;
						var arrowStart = GetArrowStartPoint(creature, creatureCenterX, creatureCenterY);

						gfx.SmoothingMode = SmoothingMode.AntiAlias;
						{
							gfx.DrawLine(m_arrowPen, arrowStart.X, arrowStart.Y, creatureCenterX, creatureCenterY);
							gfx.FillEllipse(fillBrush, entityFillRect);
							gfx.DrawString(creature.Health.ToString(), m_creatureFont, Brushes.White, entityFillRect, m_createStringFormat);
						}
						gfx.SmoothingMode = SmoothingMode.Default;
					}
					else
					{
						gfx.FillRectangle(fillBrush, entityFillRect);
					}
				}
			}
		}

		private Brush GetEntityBrush(EntityType entityType)
		{
			switch (entityType)
			{
				case EntityType.Wall: return m_wallBrush;
				case EntityType.Food: return m_foodBrush;
				case EntityType.Poison: return m_poisonBrush;
				case EntityType.Creature: return m_creatureBrush;
				case EntityType.Empty: return Brushes.White;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private PointF GetArrowStartPoint(Creature creature, float creatureCenterX, float creatureCenterY)
		{
			var radius = (BlockSize + 8) / 2F;
			var angleInRads = creature.SightVector * 360 / 8f * Math.PI / 180;

			var startX = radius * (float)Math.Cos(angleInRads) + creatureCenterX;
			var startY = radius * (float)Math.Sin(angleInRads) + creatureCenterY;

			return new PointF(startX, startY);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(Color.White);
			if (World == null)
			{
				e.Graphics.DrawString
				(
					"World is not attached",
					Font,
					Brushes.Black,
					ClientRectangle,
					m_createStringFormat
				);
				return;
			}

			var gfx = e.Graphics;

			var fieldWidth = World.Width * BlockSize;
			var fieldHeight = World.Height * BlockSize;

			var fieldX = ClientRectangle.Width / 2 - fieldWidth / 2;
			var fieldY = ClientRectangle.Height / 2 - fieldHeight / 2;

			gfx.TranslateTransform(fieldX, fieldY);
			{
				DrawGrid(gfx, fieldWidth, fieldHeight);
				DrawEntities(gfx);
			}
			gfx.ResetTransform();
		}
	}
}
