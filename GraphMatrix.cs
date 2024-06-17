using System;
using System.Collections.Generic;

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




    public void SetVertexName(int vertex, string name)
    {
        vertexNames[vertex] = name;
    }

    public void AddEdge(int source, int destination, int weight = 1)
    {
        adjacencyMatrix[source, destination] = weight;
        adjacencyMatrix[destination, source] = weight;
    }

    public void DFS(int startVertex, int targetVertex, out List<int> path)
    {
        bool[] visited = new bool[numVertices];
        Stack<int> stack = new Stack<int>();
        Stack<int> fullPath = new Stack<int>();

        visited[startVertex] = true;
        stack.Push(startVertex);

        while (stack.Count != 0)
        {
            int currentVertex = stack.Pop();
            fullPath.Push(currentVertex);

            if (currentVertex == targetVertex)
            {
                path = new List<int>(fullPath);
                path.Reverse();
                return;
            }

            for (int i = numVertices - 1; i >= 0; i--)
            {
                if (adjacencyMatrix[currentVertex, i] != 0 && !visited[i])
                {
                    visited[i] = true;
                    stack.Push(i);
                }
            }
        }

        path = null;
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
                path = new List<int>();
                for (int at = targetVertex; at != -1; at = previous[at])
                {
                    path.Add(at);
                }
                path.Reverse();
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

        path = null;
    }
}
