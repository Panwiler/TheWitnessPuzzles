﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using XnaColor = Microsoft.Xna.Framework.Color;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheWitnessPuzzles;

namespace TWPVisualizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Puzzle panel = null;

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            panel = new SymmetryPuzzle(7, 4, true, XnaColor.Aqua, XnaColor.Yellow);
            panel.Nodes[16].SetState(NodeState.Start);
            panel.Nodes[23].SetState(NodeState.Start);
            panel.Nodes[3].SetState(NodeState.Exit);
            panel.Nodes[36].SetState(NodeState.Exit);
            panel.Nodes[34].SetState(NodeState.Marked);
            panel.Edges.Find(x => x.Id == 1819)?.SetState(EdgeState.Marked);
            panel.Edges.Find(x => x.Id == 1920)?.SetState(EdgeState.Marked);
            panel.Grid[0, 2].Rule = new ColoredSquareRule(XnaColor.Yellow);
            panel.Grid[2, 0].Rule = new ColoredSquareRule(XnaColor.Aqua);
            panel.Grid[4, 0].Rule = new ColoredSquareRule(XnaColor.Yellow);
            panel.Grid[6, 1].Rule = new ColoredSquareRule(XnaColor.Yellow);
            panel.Grid[3, 3].Rule = new SunPairRule(XnaColor.Yellow);
            panel.Grid[2, 2].Rule = new EliminationRule();
            panel.Grid[3, 2].Rule = new EliminationRule();
            panel.Grid[6, 0].Rule = new TriangleRule(1);
            panel.Grid[5, 1].Rule = new TriangleRule(2);

            panel.SetSolution(new List<int>());
            
            Bitmap bmp = new Bitmap(picCanvas.Width, picCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                RenderPuzzle(panel, g);
            }

            picCanvas.Image = bmp;
        }
                
        private void btnSolve_Click(object sender, EventArgs e)
        {
            List<int> solution = Array.ConvertAll(txtSolution.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();
            panel.SetSolution(solution);

            Bitmap bmp = new Bitmap(picCanvas.Width, picCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                RenderPuzzle(panel, g);
            }

            picCanvas.Image = bmp;
        }

        private Color ConvertXnaColor(XnaColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        private void RenderPuzzle(Puzzle panel, Graphics g)
        {
            // In pixels
            int margin = 50;
            int nodeSpan = 50;
            int nodeRadius = 10;

            Brush brush = new SolidBrush(Color.Black);
            Brush errBrush = new SolidBrush(Color.Red);
            Brush errBrushA = new SolidBrush(Color.FromArgb(120, Color.Red));
            Brush errBrushE = new SolidBrush(Color.Lime);
            Brush errBrushAE = new SolidBrush(Color.FromArgb(120, Color.Lime));
            Pen pen = new Pen(brush);
            Pen penLine = new Pen(panel is SymmetryPuzzle sym ? ConvertXnaColor(sym.MainColor) : Color.Black, 5);
            Pen penLineMirr = new Pen(panel is SymmetryPuzzle sym2 ? ConvertXnaColor(sym2.MirrorColor) : Color.Black, 5);

            int width = panel.Width + 1;
            int height = panel.Height + 1;

            var errors = panel.CheckForErrors();
            var errorParts = errors.Where(x => !x.IsEliminated).Select(x => x.Source);
            var eliminatedParts = errors.Where(x => x.IsEliminated).Select(x => x.Source);
            
            for (int i = 0; i < panel.Nodes.Length; i++)
            {
                int row = i / width;
                int x = margin + (i - row * width) * nodeSpan;
                int y = margin + row * nodeSpan;

               if (panel.Nodes[i].State == NodeState.Start)
                   g.FillEllipse(brush, x - nodeRadius/2, y - nodeRadius/2, nodeRadius, nodeRadius);
                if (panel.Nodes[i].State == NodeState.Exit)
                    g.DrawEllipse(pen, x - nodeRadius / 2, y - nodeRadius / 2, nodeRadius, nodeRadius);

                if (panel.Nodes[i].State == NodeState.Marked)
                {
                    Brush br;
                    if (errorParts.Contains(panel.Nodes[i]))
                        br = errBrush;
                    else if (eliminatedParts.Contains(panel.Nodes[i]))
                        br = errBrushE;
                    else
                        br = brush;
                    g.FillEllipse(br, x - 3, y - 3, 6, 6);
                }

                g.DrawString(i.ToString(), Font, brush, x, y);
            }

            var solutionEdges = panel.SolutionEdges;
            var mirrorEdges = panel is SymmetryPuzzle sym3 ? sym3.MirrorSolutionEdges : null;
            for (int i = 0; i < panel.Edges.Count; i++)
            {
                Edge edge = panel.Edges[i];
                
                int rowA = edge.NodeA.Id / width;
                int xA = margin + (edge.NodeA.Id - rowA * width) * nodeSpan;
                int yA = margin + rowA * nodeSpan;

                int rowB = edge.NodeB.Id / width;
                int xB = margin + (edge.NodeB.Id - rowB * width) * nodeSpan;
                int yB = margin + rowB * nodeSpan;

                if (edge.State != EdgeState.Broken)
                    g.DrawLine(solutionEdges.Contains(edge) ? (mirrorEdges != null && mirrorEdges.Contains(edge) ? penLineMirr : penLine) : pen, xA, yA, xB, yB);

                if (edge.State == EdgeState.Marked)
                {
                    Brush br;
                    if (errorParts.Contains(edge))
                        br = errBrush;
                    else if (eliminatedParts.Contains(edge))
                        br = errBrushE;
                    else
                        br = brush;

                    g.FillEllipse(br, (xA + xB) / 2 - 3, (yA + yB) / 2 - 3, 6, 6);
                }
            }

            for (int i = 0; i < panel.Nodes.Length; i++)
            {
                int row = i / width;
                int x = margin + (i - row * width) * nodeSpan;
                int y = margin + row * nodeSpan;
                
                if (panel.Nodes[i].State == NodeState.Marked)
                {
                    Brush br = null;
                    if (errorParts.Contains(panel.Nodes[i]))
                        br = errBrush;
                    else if (eliminatedParts.Contains(panel.Nodes[i]))
                        br = errBrushE;

                    if(br != null)
                        g.FillEllipse(br, x - 5, y - 5, 10, 10);

                    br = panel.Nodes[i].HasColor ? new SolidBrush(ConvertXnaColor(panel.Nodes[i].Color.Value)) : brush;
                    g.FillEllipse(br, x - 3, y - 3, 6, 6);
                }

                g.DrawString(i.ToString(), Font, brush, x, y);
            }
            for (int i = 0; i < panel.Edges.Count; i++)
            {
                Edge edge = panel.Edges[i];

                int rowA = edge.NodeA.Id / width;
                int xA = margin + (edge.NodeA.Id - rowA * width) * nodeSpan;
                int yA = margin + rowA * nodeSpan;

                int rowB = edge.NodeB.Id / width;
                int xB = margin + (edge.NodeB.Id - rowB * width) * nodeSpan;
                int yB = margin + rowB * nodeSpan;

                int x = (xA + xB) / 2;
                int y = (yA + yB) / 2;

                if (edge.State == EdgeState.Marked)
                {
                    Brush br = null;
                    if (errorParts.Contains(edge))
                        br = errBrush;
                    else if (eliminatedParts.Contains(edge))
                        br = errBrushE;

                    if (br != null)
                        g.FillEllipse(br, x - 5, y - 5, 10, 10);

                    br = edge.HasColor ? new SolidBrush(ConvertXnaColor(edge.Color.Value)) : brush;

                    g.FillEllipse(br, x - 3, y - 3, 6, 6);
                }
            }

            List<Sector> sectors = panel.GetSectors();
            List<Brush> sectorBrushes = new List<Brush>
            {
                new SolidBrush(Color.Blue),
                new SolidBrush(Color.Magenta),
                new SolidBrush(Color.Green),
                new SolidBrush(Color.OrangeRed),
                new SolidBrush(Color.Cyan),
                new SolidBrush(Color.Yellow),
                new SolidBrush(Color.Gray),
                new SolidBrush(Color.Violet),
                new SolidBrush(Color.Beige),
                new SolidBrush(Color.Brown)
            };

            List<Pen> sectorPens = new List<Pen>
            {
                new Pen(Color.Blue, 3),
                new Pen(Color.Magenta, 3),
                new Pen(Color.Green, 3),
                new Pen(Color.OrangeRed, 3),
                new Pen(Color.Cyan, 3),
                new Pen(Color.Yellow, 3),
                new Pen(Color.Gray, 3),
                new Pen(Color.Violet, 3),
                new Pen(Color.Beige, 3),
                new Pen(Color.Brown, 3)
            };

            for (int i = 0; i < sectors.Count; i++)
            {
                foreach (Block block in sectors[i].Blocks)
                {
                    int row = block.Id / panel.Width;
                    int col = block.Id - row * panel.Width;

                    int x = margin + col * nodeSpan + nodeSpan / 2;
                    int y = margin + row * nodeSpan + nodeSpan / 2;

                    g.DrawRectangle(sectorPens[i], x - nodeSpan / 2 + 10, y - nodeSpan / 2 + 10, nodeSpan - 20, nodeSpan - 20);

                    if (block.Rule is ColoredSquareRule sqareRule)
                        g.FillRectangle(new SolidBrush(ConvertXnaColor(sqareRule.Color.Value)), x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                    else if (block.Rule is SunPairRule sunRule)
                        g.FillEllipse(new SolidBrush(ConvertXnaColor(sunRule.Color.Value)), x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                    else if (block.Rule is TriangleRule triRule)
                    {
                        g.FillPolygon(new SolidBrush(Color.Orange), new PointF[] {
                            new PointF(x -10, y +10),
                            new PointF(x, y -16),
                            new PointF(x +10, y +10)
                        });
                        g.DrawString(triRule.Power.ToString(), Font, brush, x - 5, y - 5);
                    }

                    if(errorParts.Contains(block))
                        g.FillRectangle(errBrushA, x - nodeSpan / 2 + 4, y - nodeSpan / 2 + 4, nodeSpan - 8, nodeSpan - 8);
                    else if (eliminatedParts.Contains(block))
                        g.FillRectangle(errBrushAE, x - nodeSpan / 2 + 4, y - nodeSpan / 2 + 4, nodeSpan - 8, nodeSpan - 8);
                }
            }
        }
    }
}
