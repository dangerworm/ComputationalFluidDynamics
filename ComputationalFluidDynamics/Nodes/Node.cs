using System;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public class Node
    {
        public NodeType NodeType { get; set; }

        public int? X { get; }
        public int? Y { get; }
        public int? Z { get; }

        public double[] Velocity { get; set; } = new double[2];
        public double[] VelocityNew { get; set; }

<<<<<<< Updated upstream
        public double[,,] FEquilibrium { get; set; }
        public double[,,] FNew { get; set; }
        public double[,,] FPrevious { get; set; }
=======
        public double[] FEquilibrium { get; set; }
        public double[] FNew { get; set; }
        public double[] FPrevious { get; set; }
>>>>>>> Stashed changes

        public double Rho { get; set; }
        public double RhoPrevious { get; set; }
        public double RhoNew { get; set; }
        public double RhoError => Math.Pow(Rho - RhoPrevious, 2);

        public Node(int? x, int? y, int? z)
        : this (NodeType.None, x, y, z, new[] {0.0, 0.0})
        {
        }

        public Node(NodeType nodeType, int? x, int? y, int? z)
            : this(nodeType, x, y, z, new[] { 0.0, 0.0 })
        {
        }

        public Node(NodeType nodeType, int? x, int? y, int? z, double[] initialVelocity)
        {
            NodeType = nodeType;

            X = x;
            Y = y;
            Z = z;

            Array.Copy(initialVelocity, Velocity, initialVelocity.Length);
        }

        public void Initialise(int numberOfVertices, double rho, double[] initialVelocity)
        {
            FEquilibrium = new double[numberOfVertices];
            FNew = new double[numberOfVertices];
            FPrevious = new double[numberOfVertices];

            for (var i = 0; i < numberOfVertices; i++)
            {
                FEquilibrium[i] = 0.0;
                FNew[i] = 0.0;
                FPrevious[i] = 0.0;
            }

            Rho = rho;
            RhoPrevious = rho;

            Array.Copy(initialVelocity, Velocity, initialVelocity.Length);
        }

        public void CalculateComponents(LatticeVectorCollection latticeVectors)
        {
            RhoNew = 0.0;
            VelocityNew = new[] { 0.0, 0.0 };

            for (var a = 0; a < FEquilibrium.Length; a++)
            {
                var value = NodeType == NodeType.Liquid ? FNew[a] : 0;

                FPrevious[a] = FNew[a];
                RhoNew += value;
                VelocityNew[0] += value * latticeVectors[a].X;
                VelocityNew[1] += value * latticeVectors[a].Y;
            }

            Rho = RhoNew;
            Velocity[0] = VelocityNew[0] / Rho;
            Velocity[1] = VelocityNew[1] / Rho;
        }

        public void ResetFNew()
        {
            for (var i = 0; i < FNew.Length; i++)
            {
                FNew[i] = 0.0;
            }
        }

        public void SetRhoPrevious()
        {
            RhoPrevious = Rho;
        }
    }
}