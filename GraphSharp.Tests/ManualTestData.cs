using System.Collections.Generic;

public static class ManualTestData
{
    public static int[][] NodesConnections = new[]{
        new[]{0,4},new[]{0,1},
        new[]{1,2},new[]{1,3},
        new[]{1,8},new[]{2,7},
        new[]{2,9},new[]{3,7},
        new[]{4,5},new[]{5,0},
        new[]{5,1},new[]{5,3},
        new[]{5,6},new[]{6,3},
        new[]{7,1},new[]{7,9},
        new[]{8,9},new[]{9,3}
    };
    public static int[][][] ExpectedOrder =
    new[]{
        new[]{
            new[]{0, 7},
            new[]{1, 4, 9},
            new[]{2, 3, 5, 8},
            new[]{0, 1, 3, 6, 7, 9},
            new[]{1, 2, 3, 4, 7, 8, 9},
            new[]{1, 2, 3, 5, 7, 8, 9}
        },
        new[]{
            new[]{1,5},
            new[]{0,1,2,3,6,8},
            new[]{1,2,3,4,7,8,9},
            new[]{1,2,3,5,7,8,9},
            new[]{0,1,2,3,6,7,8,9},
            new[]{1,2,3,4,7,8,9}
        }
    };
    public static IDictionary<int, IEnumerable<int>> TestConnectionsList = 
        new Dictionary<int,IEnumerable<int>>{
            {1,new[]{2,4,6,13}},
            {2,new[]{3,6,7,12}},
            {3,new[]{4,19}},
            {5,new[]{9,15,20}},
            {6,new[]{12,13,18}},
            {7,new[]{9,12,15}},
            {8,new[]{9,12,14,16}},
            {9,new[]{10}},
            {10,new[]{14,17,20}},
            {11,new[]{12,16,18}},
            {14,new[]{16}},
            {15,new[]{19}},
            {16,new[]{17}}
        };
    /// <summary>
    /// Values, that expected after calling method MakeSources(1,14) on graph builded on top of TestConnectionsList and made bidirected
    /// </summary>
    public static IEnumerable<(int sourceId, int[] targetren)> AfterMakeSourcesExpected = 
        new[]{
            (1, new[]{2,4,6,13}),
            (2, new[]{3,6,7,12}),
            (3, new[]{19}),
            (4, new[]{3}),
            (5, new[]{15}),
            (6, new[]{2,12,13,18}),
            (7, new[]{9,12,15}),
            (8, new[]{9,12,16}),
            (9, new[]{5,7}),
            (10,new[]{9,17,20}),
            (11,new[]{12,18}),
            (12,new[]{7,11}),
            (13,new[]{6}),
            (14,new[]{8,10,16}),
            (15,new[]{5,19}),
            (16,new[]{8,11,17}),
            (18,new[]{11}),
            (19,new[]{15}),
            (20,new[]{5}),
        };
}