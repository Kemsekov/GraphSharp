using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Extensions;
using Xunit;

namespace GraphSharp.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void FindFirstNMinimalElements_Works()
        {
            var count = 7;
            var rand = new Random(1);
            var randArray = Enumerable.Range(0, 50).Select(x => rand.Next(1000)).ToArray();
            var sorted = randArray.Clone() as int[];
            Array.Sort(sorted, (t1, t2) => t1 - t2);
            var result = randArray.FindFirstNMinimalElements(count,(t1,t2)=>t1-t2);
            Assert.Equal(sorted.Take(count), result);
        }
        [Fact]
        public void FindFirstNMinimalElements_SkipWorks()
        {
            var count = 7;
            var rand = new Random(1);
            var randArray = Enumerable.Range(0, 50).Select(x => rand.Next(1000)).ToArray();
            var sorted = randArray.Clone() as int[];
            Array.Sort(sorted, (t1, t2) => t1 - t2);
            var result = randArray.FindFirstNMinimalElements(count,(t1,t2)=>t1-t2,(x)=>x%2==0);
            Assert.Equal(sorted.Where(x=>x%2!=0).Take(count), result);
        }
        [Fact]
        public void FindFirstNMinimalElements_ReturnsEmptyWhenNIsZero()
        {
            var result = new[]{1,2,3}.FindFirstNMinimalElements(0,(t1,t2)=>t1-t2);
            Assert.Empty(result);
        }
        [Fact]
        public void FindFirstNMinimalElements_ReturnsRightCountOfElements(){
            var result = new[]{1,2,3,4,5,6}.FindFirstNMinimalElements(3,(t1,t2)=>t1-t2);
            Assert.Equal(result.Count(),3);
        }
    }
}