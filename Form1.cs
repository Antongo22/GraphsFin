using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GraphsFin
{
    public partial class Form1 : Form
    {
        private const int Rows = 5;
        private const int Cols = 7;
        private const int VertexSize = 15;
        private const int Padding = 20;
        private GraphMatrix graph;
        private List<Point> points;
        private List<int> path;
        private TextBox pathTextBox;

        public Form1()
        {
            InitializeComponent();
            InitializeGraph();

            pathTextBox = new TextBox();
            pathTextBox.Multiline = true;
            pathTextBox.Width = 200;
            pathTextBox.Height = 100;
            pathTextBox.Location = new Point(10, 10);
            this.Controls.Add(pathTextBox);
        }

        private void InitializeGraph()
        {
            int numVertices = Rows * Cols;
            graph = new GraphMatrix(numVertices);
            points = new List<Point>();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int vertexIndex = i * Cols + j;
                    graph.SetVertexName(vertexIndex, $"V{vertexIndex}");
                    points.Add(new Point(Padding + j * VertexSize * 4, Padding + i * VertexSize * 4));
                }
            }

            GenerateRandomGraph();
        }

        private void GenerateRandomGraph()
        {
            int numVertices = Rows * Cols;
            Random rand = new Random();
            int maxAttempts = 10000;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Array.Clear(graph.adjacencyMatrix, 0, graph.adjacencyMatrix.Length);

                for (int i = 0; i < numVertices; i++)
                {
                    int edges = rand.Next(1, 3);
                    int currentEdges = graph.GetEdgeCount(i);

                    while (currentEdges < edges)
                    {
                        int dest = rand.Next(numVertices);

                        if (i != dest && graph.GetEdgeCount(dest) < 2 && graph.adjacencyMatrix[i, dest] == 0)
                        {
                            graph.AddEdge(i, dest);
                            currentEdges++;
                        }
                    }
                }

                if (HasPathFromStartToEnd(minPathLength: 3))
                {
                    drawingPanel.Invalidate();
                    return;
                }

                attempts++;
            }

            MessageBox.Show("Не удалось сгенерировать граф с путём от начала до конца через 3 и более точки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool HasPathFromStartToEnd(int minPathLength)
        {
            List<int> path;
            graph.BFS(0, Rows * Cols - 1, out path);

            return path != null && path.Count >= minPathLength;
        }

        private void GraphVisualization_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen edgePen = new Pen(Color.Black, 3);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (graph.adjacencyMatrix[i, j] != 0)
                    {
                        g.DrawLine(edgePen, points[i], points[j]);
                    }
                }
            }

            Font font = new Font("Arial", 10);
            Brush brush = Brushes.Black;

            for (int i = 0; i < points.Count; i++)
            {
                g.FillEllipse(Brushes.Yellow, points[i].X - VertexSize / 2, points[i].Y - VertexSize / 2, VertexSize, VertexSize);
                string vertexName = graph.GetVertexName(i);
                SizeF nameSize = g.MeasureString(vertexName, font);
                PointF namePosition = new PointF(points[i].X - nameSize.Width / 2, points[i].Y - nameSize.Height / 2);
                g.DrawString(vertexName, font, brush, namePosition);
            }

            if (path != null)
            {
                Pen pathPen = new Pen(Color.Red, 3);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(pathPen, points[path[i]], points[path[i + 1]]);
                }
            }
        }

        private void btnFindPath_Click(object sender, EventArgs e)
        {
            graph.BFS(0, Rows * Cols - 1, out path);
            DisplayPath();
            drawingPanel.Invalidate();

            if (path != null)
            {
                StringBuilder pathMessage = new StringBuilder();
                foreach (int vertex in path)
                {
                    pathMessage.Append(graph.GetVertexName(vertex)).Append(" ");
                }
                label1.Text = $"Путь: {pathMessage.ToString()}";
            }
            else
            {
                MessageBox.Show("Путь не найден.", "Путь не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DisplayPath()
        {
            if (path != null)
            {
                StringBuilder pathInfo = new StringBuilder();
                pathInfo.AppendLine("Найденный путь:");

                for (int i = 0; i < path.Count; i++)
                {
                    pathInfo.AppendLine(graph.GetVertexName(path[i]));
                }

                pathTextBox.Text = pathInfo.ToString();
            }
            else
            {
                pathTextBox.Text = "Путь не найден.";
            }
        }

        private void btnGenerateGraph_Click(object sender, EventArgs e)
        {
            graph = null;
            points.Clear();
            path = null;
            pathTextBox.Text = string.Empty;
            drawingPanel.Invalidate();

            InitializeGraph();
            drawingPanel.Invalidate();
        }
    }

    public class GraphMatrix
    {
        public int[,] adjacencyMatrix;
        private int numVertices;
        private string[] vertexNames;

        public GraphMatrix(int numVertices)
        {
            this.numVertices = numVertices;
            adjacencyMatrix = new int[numVertices, numVertices];
            vertexNames = new string[numVertices];

            for (int i = 0; i < numVertices; i++)
            {
                vertexNames[i] = "";
            }
        }

        public void SetVertexName(int vertex, string name)
        {
            vertexNames[vertex] = name;
        }

        public string GetVertexName(int vertex)
        {
            return vertexNames[vertex];
        }

        public void AddEdge(int source, int destination, int weight = 1)
        {
            adjacencyMatrix[source, destination] = weight;
            adjacencyMatrix[destination, source] = weight;
        }

        public int GetEdgeCount(int vertex)
        {
            int count = 0;
            for (int i = 0; i < adjacencyMatrix.GetLength(1); i++)
            {
                if (adjacencyMatrix[vertex, i] != 0 || adjacencyMatrix[i, vertex] != 0)
                {
                    count++;
                }
            }
            return count;
        }

        public bool BFS(int startVertex, int targetVertex, out List<int> path)
        {
            bool[] visited = new bool[numVertices];
            Queue<int> queue = new Queue<int>();
            int[] previous = new int[numVertices];

            for (int i = 0; i < numVertices; i++)
            {
                previous[i] = -1;
            }

            visited[startVertex] = true;
            queue.Enqueue(startVertex);

            while (queue.Count != 0)
            {
                int currentVertex = queue.Dequeue();

                if (currentVertex == targetVertex)
                {
                    path = ConstructPath(previous, startVertex, targetVertex);
                    return true;
                }

                for (int i = 0; i < numVertices; i++)
                {
                    if (adjacencyMatrix[currentVertex, i] != 0 && !visited[i])
                    {
                        visited[i] = true;
                        queue.Enqueue(i);
                        previous[i] = currentVertex;
                    }
                }
            }

            path = null;
            return false;
        }

        private List<int> ConstructPath(int[] previous, int startVertex, int targetVertex)
        {
            List<int> path = new List<int>();
            for (int at = targetVertex; at != -1; at = previous[at])
            {
                path.Add(at);
            }
            path.Reverse();
            return path;
        }
    }
}
