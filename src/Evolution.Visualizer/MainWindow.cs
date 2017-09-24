using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Evolution.Core;

namespace Evolution.Visualizer
{
	public partial class MainWindow : Form
	{
		private readonly World m_world = new World(42, 22);
		private readonly ManualResetEvent m_waiter = new ManualResetEvent(true);

		private int m_generation;
		private int m_generationLifetime;
		private int m_maxGenerationLifetime;

		private readonly LinkedList<int> m_generationLifetimeStats = new LinkedList<int>();

		private bool m_fastMode;

		public MainWindow()
		{
			InitializeComponent();
			evolutionCanvas1.World = m_world;

			for (var x = 0; x < m_world.Width; x++)
			{
				m_world[x, 0] = new Wall();
				m_world[x, m_world.Height - 1] = new Wall();
			}
			for (var y = 0; y < m_world.Height; y++)
			{
				m_world[0, y] = new Wall();
				m_world[m_world.Width - 1, y] = new Wall();
			}

			m_world[10, 0] = new Wall();
			m_world[10, 1] = new Wall();
			m_world[10, 2] = new Wall();
			m_world[10, 3] = new Wall();
			m_world[10, 4] = new Wall();
			m_world[10, 5] = new Wall();
			m_world[10, 6] = new Wall();

			m_world[25, 10]= new Wall();
			m_world[25, 11]= new Wall();
			m_world[25, 12]= new Wall();
			m_world[25, 13] = new Wall();
			m_world[25, 14] = new Wall();
			m_world[25, 15] = new Wall();
			m_world[25, 16] = new Wall();

			m_world.PrepareWorld();

			Load += MainWindow_Load;

			evolutionCanvas1.Click += (s, e) =>
			{
				m_fastMode = !m_fastMode;
			};
		}

		private void MainWindow_Load(object sender, EventArgs e)
		{
			new Thread(() =>
			{
				try
				{
					while (true)
					{
						m_waiter.WaitOne();

						for (var i = 0; i < (m_fastMode ? 10 : 1); i++)
						{
							EvolutionIteration();
						}

						try
						{
							Invoke(new Action(() =>
							{
								evolutionCanvas1.Invalidate();
								Text = $@"Generation: {m_generation}, LT: {m_maxGenerationLifetime}";

								textBox2.Text = m_generationLifetime.ToString();
								textBox1.Text = string.Join(Environment.NewLine, m_generationLifetimeStats.Select(x => x));
							}));
						}
						catch
						{
							// Ignore
						}

						Thread.Sleep(m_fastMode ? 16 : 80);
					}
				}
				catch(Exception ex)
				{
					ex = ex;
				}
			}) { IsBackground = true }.Start();
		}

		private bool EvolutionIteration()
		{
			m_world.Iteration();
			m_generationLifetime++;

			if (m_world.LiveCreatures.Count <= 8)
			{
				var creatures = new Creature[64];
				for (var i = 0; i < 8; i++)
				{
					for (var j = 0; j < 8; j++)
					{
						Creature child;
						if (m_world.LiveCreatures.Count == 0)
						{
							child = new Creature();
						}
						else if (i > m_world.LiveCreatures.Count - 1)
						{
							child = new Creature(m_world.LiveCreatures[0].CopyGenome());
						}
						else
						{
							child = new Creature(m_world.LiveCreatures[i].CopyGenome());
						}
						creatures[i * 8 + j] = child;
					}
				}

				for (var i = 0; i < 16; i++)
				{
					creatures[i].Mutate();
				}

				m_world.LiveCreatures.Clear();
				m_world.PrepareWorld(creatures);
				
				m_generationLifetimeStats.AddFirst(m_generationLifetime);
				if (m_generationLifetimeStats.Count > 10) m_generationLifetimeStats.RemoveLast();

				if (m_maxGenerationLifetime < m_generationLifetime)
				{
					m_maxGenerationLifetime = m_generationLifetime;
				}
				m_generationLifetime = 0;
				m_generation++;
				return true;
			}

			return false;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			m_waiter.Reset();

			var generations = 0;
			while (generations < 100)
			{
				if (EvolutionIteration()) generations++;
			}

			m_waiter.Set();
		}
	}
}
