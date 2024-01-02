using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unchase.Satsuma.TSP;
using Unchase.Satsuma.TSP.Contracts;
namespace GraphSharp.Graphs;

/// <summary>
/// Small opt2 algorithm. It is small because it uses smaller subset of node permutations.<br/>
/// And it is random because it uses parallel computation to get subset of edges that is swappable.<br/>
/// In general it gives better results than <see cref="Unchase.Satsuma.TSP.Opt2Tsp{TNode}"/> 
/// and with reasonable time depending on <see cref="SmallRandomOpt2{TNode}.MaxPermutationsPerNode"/>
/// </summary>
public class SmallRandomOpt2<TNode> : ITsp<TNode>
{
    private readonly List<TNode> _tour;
    ///<inheritdoc/>
    public Func<TNode, TNode, double> Cost { get; }
    ///<inheritdoc/>
    public IEnumerable<TNode> Tour => _tour;
    ///<inheritdoc/>
    public double TourCost { get; private set; }
    /// <summary>
    /// Max amount of searching permutations per node.<br/>
    /// Set it as some multiple of tour length. For example tour.Count/4
    /// </summary>
    public int MaxPermutationsPerNode{get;set;}
    /// <summary>
    /// </summary>
    /// <param name="cost">Cost function</param>
    /// <param name="tour">starting tour</param>
    /// <param name="tourCost">tour cost</param>
    public SmallRandomOpt2(Func<TNode, TNode, double> cost, IEnumerable<TNode> tour, double? tourCost)
    {
        Cost = cost;
        _tour = tour.ToList();
        TourCost = tourCost ?? TspUtils.GetTourCost(_tour, cost);
        MaxPermutationsPerNode=_tour.Count/4;
    }
    double SwapGain(int i, int j)
    {
        var pair1 = Cost(_tour[i], _tour[j]);
        var pair2 = Cost(_tour[i + 1], _tour[j + 1]);
        var pair3 = Cost(_tour[i], _tour[i + 1]);
        var pair4 = Cost(_tour[j], _tour[j + 1]);

        double gain = pair1 + pair2 - (pair3 + pair4);
        return gain;
    }
    double SwapGain(int i, int j,IList<TNode> tour)
    {
        var pair1 = Cost(tour[i], tour[j]);
        var pair2 = Cost(tour[i + 1], tour[j + 1]);
        var pair3 = Cost(tour[i], tour[i + 1]);
        var pair4 = Cost(tour[j], tour[j + 1]);

        double gain = pair1 + pair2 - (pair3 + pair4);
        return gain;
    }
    /// <summary>
    /// Steps an algorithm iteration
    /// </summary>
    /// <returns>True if improved</returns>
    public bool Step()
    {
        bool result = false;
        var swaps = new List<(int i, int j, double gain)>();
        
        Parallel.For(0, _tour.Count - 3, i =>
        {
            // var localTour = threadLocalTour.Value;
            // localTour.Clear();
            // localTour.AddRange(_tour);
            // var localCost = this.TourCost;

            var localSwaps = new List<(int i, int j, double gain)>();

            int num = _tour.Count - ((i != 0) ? 1 : 2);

            var rand = new Random();
            var start = rand.Next();
            // j must be in [i+2;num]
            var maxK = num - i - 2;

            var upJ = num - i - 2;
            var add = i+2;
            // System.Console.WriteLine(maxK);
            maxK = Math.Min(maxK,MaxPermutationsPerNode);
            for (int k = 0; k < maxK; k++)
            {
                var j = (start + k) % upJ + add;

                double gain = SwapGain(i, j);
                if (gain < 0)
                {
                    localSwaps.Add((i, j, gain));
                }
            }
            lock (swaps)
                swaps.AddRange(localSwaps);
        });

        var orderedSwaps = swaps.OrderBy(n => n.gain);
        var improved = 0;
        foreach (var (i, j, gain) in orderedSwaps)
        {
            var newGain = SwapGain(i, j);
            if (newGain < 0)
            {
                TourCost += newGain;
                _tour.Reverse(i + 1, j - i);
                result = true;
                improved++;
            }
        }
        return result;
    }

    /// <summary>
    /// Performs steps until no improvement is achieved
    /// </summary>
    public void Run()
    {
        while (Step())
        {
        }
    }
    /// <summary>
    /// Performs steps until no improvement is achieved or max iterations reached
    /// </summary>
    public void Run(int maxIterations)
    {
        while (maxIterations-- > 0 && Step())
        {
        }
    }
}