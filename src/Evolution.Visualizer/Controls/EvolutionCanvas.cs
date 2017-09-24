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

		private readonly Pen m_arrowPen = new Pen(Color.FromArgb(0xC9, 0xCA, 0xCB), 3) { StartCap = LineCap.ArrowAnchor };
		private readonly SolidBrush m_creatureBrush = new SolidBrush(Color.FromArgb(0x23, 0xAE, 0xEA));
		private readonly SolidBrush m_foodBrush = new SolidBrush(Color.FromArgb(0x9B,0xC5,0x3D));
		private readonly SolidBrush m_poisonBrush = new SolidBrush(Color.FromArgb(0xFA,0x8D,0x29));
		private readonly SolidBrush m_wallBrush = new SolidBrush(Color.FromArgb(0xC9, 0xCA, 0xCB));

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
					new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }
				);
				return;
			}

			var gfx = e.Graphics;

			var fieldWidth = World.Width * BlockSize;
			var fieldHeight = World.Height * BlockSize;

			var fieldX = ClientRectangle.Width / 2 - fieldWidth / 2;
			var fieldY = ClientRectangle.Height / 2 - fieldHeight / 2;

			gfx.TranslateTransform(fieldX + 1, fieldY + 1);

			for (var mx = 0; mx < World.Width; mx++)
			{
				for (var my = 0; my < World.Height; my++)
				{
					var entityRect = new Rectangle(mx * BlockSize, my * BlockSize, BlockSize, BlockSize);
					gfx.DrawRectangle(new Pen(Color.FromArgb(240, 240, 240)), entityRect);

					var entity = World[mx, my];
					if (entity != null)
					{
						var entityFillRect = new Rectangle(entityRect.X + 1, entityRect.Y + 1, entityRect.Width - 1, entityRect.Height - 1);
						//var entityFillRect = entityRect;
						Brush fillBrush;

						switch (entity.EntityType)
						{
							case EntityType.Wall:
								fillBrush = m_wallBrush;
								break;
							case EntityType.Food:
								fillBrush = m_foodBrush;
								break;
							case EntityType.Poison:
								fillBrush = m_poisonBrush;
								break;
							case EntityType.Creature:
								fillBrush = m_creatureBrush;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						if (entity is Creature creature)
						{
							int startX;
							int startY;

							if (creature.SightVector >= 0 && creature.SightVector <= 2)
							{
								startY = entityFillRect.Top - 5;
							}
							else if (creature.SightVector >= 4 && creature.SightVector <= 6)
							{
								startY = entityFillRect.Bottom + 5;
							}
							else startY = entityFillRect.Y + entityFillRect.Height / 2;

							if (creature.SightVector >= 2 && creature.SightVector <= 4)
							{
								startX = entityFillRect.Right + 5;
							}
							else if (creature.SightVector == 0 || creature.SightVector == 6 || creature.SightVector == 7)
							{
								startX = entityFillRect.Left - 5;
							}
							else startX = entityFillRect.X + entityFillRect.Width / 2;

							gfx.SmoothingMode = SmoothingMode.AntiAlias;
							gfx.DrawLine
							(
								m_arrowPen,
								startX,
								startY,
								entityFillRect.X + entityFillRect.Width / 2,
								entityFillRect.Y + entityFillRect.Height / 2
							);
							gfx.SmoothingMode = SmoothingMode.Default;

							gfx.FillRectangle(fillBrush, entityFillRect);
							gfx.DrawString(creature.Health.ToString(), Font, Brushes.White, entityFillRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
						}
						else
						{
							gfx.FillRectangle(fillBrush, entityFillRect);
						}
					}
				}
			}

			gfx.ResetTransform();
		}
	}
}
