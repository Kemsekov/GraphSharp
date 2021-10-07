using System;
using System.Collections.Generic;

namespace GraphSharp.Extensions
{
    public static class IListExtension
    {
        public static void Shuffle<T>(this IList<T> list,int shuffle_element_count = -1,Random rng = null)  
        {
            if(rng is null) rng = new Random();

            int n = shuffle_element_count>0 ? shuffle_element_count : list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}