using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GraphSharp.Tests
{
    public class HelpersTests
    {
        [Fact]
        public void FindFirstNMinimalElements_Works()
        {
            var count = 7;
            var rand = new Random(1);
            var randArray = Enumerable.Range(0, 50).Select(x => rand.Next(1000)).ToArray();
            var sorted = randArray.Clone() as int[];
            Array.Sort(sorted, (t1, t2) => t1 - t2);
            var result = 
                GraphSharp.Helpers.Helpers.FindFirstNMinimalElements(count,randArray,(t1,t2)=>t1-t2);
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
            var result = 
                GraphSharp.Helpers.Helpers.FindFirstNMinimalElements(count,randArray,(t1,t2)=>t1-t2,(x)=>x%2==0);
            Assert.Equal(sorted.Where(x=>x%2!=0).Take(count), result);
        }
        [Fact]
        public void FindFirstNMinimalElements_ReturnsEmptyWhenNIsZero()
        {
            var result = 
                GraphSharp.Helpers.Helpers.FindFirstNMinimalElements(0,new[]{1,2,3},(t1,t2)=>t1-t2);
            Assert.Empty(result);
        }
        [Fact]
        public void FindFirstNMinimalElements_ReturnsRightCountOfElements(){
            var result = 
                GraphSharp.Helpers.Helpers.FindFirstNMinimalElements(3,new[]{1,2,3,4,5,6},(t1,t2)=>t1-t2);
            Assert.Equal(result.Count(),3);

        }

        [Fact]
        public void RandNextExtension_Works()
        {
            var rand = new Random();
            var from = rand.Next(100);
            var to = rand.Next(100)+from;
            for(int i = 0;i<100;i++){
                var value = rand.Next(from,to);
                Assert.True(value>=from && value<=to);
            }
        }
    }
}