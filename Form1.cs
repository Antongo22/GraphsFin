using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GraphsFin
{
    public partial class Form1 : Form
    {
        private const int Rows = 3; // Количество строк
        private const int Cols = 5; // Количество столбцов
        private const int VertexSize = 20;
        private const int Padding = 50; // Увеличенное расстояние между точками
        private GraphMatrix graph;
        private List<Point> points;
        private List<int> path;

        public Form1()
        {
            InitializeComponent();
            InitializeGraph();
        }

        private void InitializeGraph()
        {
            int numVertices = Rows * Cols;
            graph = new GraphMatrix(numVertices);
            points = new List<Point>();

            // Генерация вершин
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int vertexIndex = i * Cols + j;
                    graph.SetVertexName(vertexIndex, vertexIndex.ToString());
                    points.Add(new Point(Padding + j * VertexSize * 4, Padding + i * VertexSize * 4));
                }
            }

            // Добавляем случайные дополнительные ребра
            GenerateRandomGraph();
        }

        private void GenerateRandomGraph()
        {
            int numVertices = Rows * Cols;
            Random rand = new Random();

            int maxAttempts = 100; // Максимальное количество попыток генерации
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // Очищаем матрицу смежности
                Array.Clear(graph.adjacencyMatrix, 0, graph.adjacencyMatrix.Length);

                // Создаем случайные дополнительные ребра
                for (int i = 0; i < numVertices; i++)
                {
                    int edges = rand.Next(0, 3); // От 0 до 2 рёбер
                    int currentEdges = graph.GetEdgeCount(i); // Текущее количество рёбер для вершины

                    while (currentEdges < edges)
                    {
                        // Генерируем случайное направление (включая диагонали)
                        int dx = rand.Next(-1, 2);
                        int dy = rand.Next(-1, 2);

                        int destRow = i / Cols + dy;
                        int destCol = i % Cols + dx;

                        if (destRow >= 0 && destRow < Rows && destCol >= 0 && destCol < Cols)
                        {
                            int dest = destRow * Cols + destCol;
                            if (i != dest && graph.GetEdgeCount(dest) < 2 && graph.adjacencyMatrix[i, dest] == 0)
                            {
                                graph.AddEdge(i, dest);
                                currentEdges++;
                            }
                        }
                    }
                }

                if (HasPathFromStartToEnd())
                {
                    return;
                }

                attempts++;
            }

            MessageBox.Show("Не удалось сгенерировать граф с путём от начала до конца.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool HasPathFromStartToEnd()
        {
            List<int> path;
            graph.BFS(0, Rows * Cols - 1, out path);
            return path != null;
        }

        private void GraphVisualization_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Рисуем ребра
            Pen edgePen = new Pen(Color.Black, 3); // Увеличенная толщина ребер
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

            // Рисуем вершины
            foreach (Point p in points)
            {
                g.FillEllipse(Brushes.Black, p.X - VertexSize / 2, p.Y - VertexSize / 2, VertexSize, VertexSize);
            }

            // Рисуем путь, если он найден
            if (path != null)
            {
                Pen pathPen = new Pen(Color.Red, 3); // Увеличенная толщина пути
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(pathPen, points[path[i]], points[path[i + 1]]);
                }
            }
        }

        private void btnFindPath_Click(object sender, EventArgs e)
        {
            graph.BFS(0, Rows * Cols - 1, out path);
            drawingPanel.Invalidate(); // Перерисовываем форму для отображения пути
        }

        private void btnGenerateGraph_Click(object sender, EventArgs e)
        {
            graph = null;
            points.Clear();
            path = null;
            drawingPanel.Invalidate();

            InitializeGraph();
            drawingPanel.Invalidate(); // Перерисовываем форму для отображения нового графа
        }
    }

    // Добавьте класс GraphMatrix здесь
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

        public void AddEdge(int source, int destination, int weight = 1)
        {
            adjacencyMatrix[source, destination] = weight;
            adjacencyMatrix[destination, source] = weight;
        }

        public int GetEdgeCount(int vertex)
        {
            int count = 0;
            for (int i = 0; i < numVertices; i++)
            {
                if (adjacencyMatrix[vertex, i] != 0)
                {
                    count++;
                }
            }
            return count;
        }

        public void BFS(int startVertex, int targetVertex, out List<int> path)
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
                    return;
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

            path = null; // Путь не найден
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
