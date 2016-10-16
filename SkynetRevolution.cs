namespace SkynetRevolution
{
	using System;
	using System.Linq;
	using System.IO;
	using System.Text;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Solution to the Skynet Revolution Episode 1 Coding Game
	/// https://www.codingame.com/training/medium/skynet-revolution-episode-1
	/// 
	/// Set up: reads input to construct the game graph's links and exit nodes
	/// Plays the game: In the game loop, reads the enemy agent's position and responds by severing a node in the graph,
	///		preventing the enemy agent from escaping
	/// </summary>
	class Player
	{
		static void Main(string[] args)
		{
			string[] inputs;
			inputs = Console.ReadLine().Split(' ');
			int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
			int L = int.Parse(inputs[1]); // the number of links
			int E = int.Parse(inputs[2]); // the number of exit gateways

			NodeGraph<int> skyNetwork = new NodeGraph<int>(); //Stores the final game graph
			List<int> exitNodes = new List<int>();  //stores the list of exit nodes to block

			for (int i = 0; i < L; i++)  //Input loop - build game graph
			{
				inputs = Console.ReadLine().Split(' ');
				int N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
				int N2 = int.Parse(inputs[1]);
				skyNetwork.AddLink(N1, N2);
			}

			for (int i = 0; i < E; i++) //Input loop - build list of exit nodes
			{
				int EI = int.Parse(Console.ReadLine()); // the index of an exit gateway node
				exitNodes.Add(EI);
			}

			while (true) //Game Loop - find enemy agent's current position and sever a node to prevent it from leaving
			{
				int SI = int.Parse(Console.ReadLine()); // The index of the node on which the enemy agent is positioned this turn
				Tuple<int, int> linkToSever = skyNetwork.FindSeverLink(SI, exitNodes); //find the best link to sever this iteration
				Console.WriteLine("{0} {1}", linkToSever.Item1, linkToSever.Item2); // Write action to Console
			}
		}
	}

	/// <summary>
	/// Class representing a graph of linked nodes
	///		Links are bidirectional and unweighted
	/// </summary>
	/// <typeparam name="T">The element type of the graph</typeparam>
	class NodeGraph<T>
	{
		List<Node<T>> _NodeList;

		public NodeGraph()
		{
			_NodeList = new List<Node<T>>();
		}

		/// <summary>
		/// Creates a link between two nodes. These nodes may be new or already stored in the graph
		/// </summary>
		/// <param name="root">First node</param>
		/// <param name="link">Second node</param>
		public void AddLink(T root, T link)
		{
			bool done = false;
			Node<T> rootNode = null, linkNode = null;

			if (_NodeList.Any())
			{
				foreach (Node<T> curNode in _NodeList) //Loop through nodes
				{
					if (curNode.Item.Equals(root)) rootNode = curNode;
					else if (curNode.Item.Equals(link)) linkNode = curNode;

					if (rootNode != null && linkNode != null) //found both in _NodeList - add the link between them
					{
						rootNode.AddChild(linkNode);
						done = true;
					}
				}
			}
			if (!done && (rootNode != null || linkNode != null)) //found one node but not the other
			{
				if (rootNode == null) rootNode = new Node<T>(root); _NodeList.Add(rootNode);
				if (linkNode == null) linkNode = new Node<T>(link); _NodeList.Add(linkNode);
				rootNode.AddChild(linkNode);
				done = true;
			}
			else if (!done) //neither node can be found in the graph. both will be created
			{
				rootNode = new Node<T>(root);
				linkNode = new Node<T>(link);
				_NodeList.Add(rootNode);
				_NodeList.Add(linkNode);
				rootNode.AddChild(linkNode); //Both will be marked as children of each other
			}
		}

		/// <summary>
		/// Find the best node to sever from the graph to prevent the enemy agent from reaching an exit node
		/// If the agent is adjacent to an exit node we will sever that one
		/// Otherwise we can just sever any node adjacent to the agent
		/// </summary>
		/// <param name="curPos">the enemy agent's current position</param>
		/// <param name="exitNodes">list of nodes to prevent agent from reaching</param>
		/// <returns>tuple of two nodes representing a severed link</returns>
		public Tuple<T, T> FindSeverLink(T curPos, List<T> exitNodes)
		{
			Node<T> curNode = FindNode(curPos);
			if (curNode != null)
			{
				//compare the agent's current position to all exit nodes
				foreach (T exit in exitNodes)
				{
					if (IsLink(curNode, FindNode(exit), true))
					{
						Console.Error.WriteLine("Skynet agent adjacent to exit: {0}", exit);
						return Tuple.Create(curPos, exit);
					}
				}

				//if the agent isn't adjacent to any exit node, then just return any link from the agent's current node
				return Tuple.Create(curPos, curNode.RemoveFirst().Item);
			}
			return Tuple.Create(default(T), default(T));
		}

		/// <summary>
		/// Determines if there is a link between two nodes. 
		/// Optionally, remove the link between the two nodes.
		/// </summary>
		/// <param name="n1">first node</param>
		/// <param name="n2">second node</param>
		/// <param name="shouldRemove">whether or not to remove the link, if found</param>
		/// <returns>true if link exists, false otherwise</returns>
		bool IsLink(Node<T> n1, Node<T> n2, bool shouldRemove)
		{
			if (n1 == null || n2 == null) return false;
			if (n1.Children.Contains(n2) || n2.Children.Contains(n1))
			{
				if (shouldRemove)
				{
					n1.RemoveLink(n2); //this will remove the link both ways
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Search the tree for a Node containing the input value
		/// </summary>
		/// <param name="toFind">value to find</param>
		/// <returns>Node containing the input value</returns>
		Node<T> FindNode(T toFind)
		{
			return _NodeList.Find(x => x.Item.Equals(toFind));
		}
	}

	/// <summary>
	/// Class representing doubly-linked nodes
	/// </summary>
	/// <typeparam name="T">The element type of the node</typeparam>
	class Node<T>
	{
		public T Item { get; private set; }
		public List<Node<T>> Children { get; private set; }

		public Node()
		{
			Children = new List<Node<T>>();
		}

		public Node(T item)
			: this()
		{
			Item = item;
		}

		/// <summary>
		/// adds the links between this node and another node
		/// </summary>
		/// <param name="newNode">Node to link to</param>
		public void AddChild(Node<T> newNode)
		{
			Children.Add(newNode);
			newNode.Children.Add(this);
		}

		/// <summary>
		/// Removes the links between this node and another node
		/// </summary>
		/// <param name="child">Node to remove link to</param>
		public void RemoveLink(Node<T> child)
		{
			if (child != null) child.Children.Remove(this);
			Children.Remove(child);
		}

		/// <summary>
		/// Removes and returns this node's first child
		/// </summary>
		/// <returns>the removed node</returns>
		public Node<T> RemoveFirst()
		{
			Node<T> firstChild = Children[1];
			RemoveLink(firstChild);
			return firstChild;
		}
	}
}
