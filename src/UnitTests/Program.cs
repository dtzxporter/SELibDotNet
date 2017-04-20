using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SELib;

namespace UnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            // SELib Unit Test
            Console.WriteLine("SELib Unit Tests\n");

            #region SEAnim

            {
                // Log
                Console.Write("-- Test 1   ");
                // Make it
                var anim = new SEAnim();
                // Add some keys
                anim.AddTranslationKey("shoulder", 0, 0, 0, 0);
                anim.AddTranslationKey("shoulder", 5, 1, 1, 1);
                anim.AddTranslationKey("shoulder", 10, 10, 10, 10);
                anim.AddTranslationKey("shoulder", 30, 20, 20, 20);
                anim.AddTranslationKey("shoulder", 40, 30, 30, 30);

                // Save it
                anim.Write("test1.seanim");
                // Done
                Console.WriteLine("DONE!");
            }

            {
                // Log
                Console.Write("-- Test 2   ");
                // Make it
                var anim = new SEAnim();
                // Add some keys
                anim.AddTranslationKey("shoulder", 0, 0, 0, 0);
                anim.AddTranslationKey("shoulder", 5, 1, 1, 1);
                anim.AddTranslationKey("shoulder", 10, 10, 10, 10);
                anim.AddTranslationKey("shoulder", 30, 20, 20, 20);
                anim.AddTranslationKey("shoulder", 40, 30, 30, 30);
                // Add some scale
                anim.AddScaleKey("shoulder", 0, 1, 1, 1);
                anim.AddScaleKey("shoulder", 50, 3, 3, 3);

                // Save it
                anim.Write("test2.seanim");
                // Done
                Console.WriteLine("DONE!");
            }

            {
                // Log
                Console.Write("-- Test 3   ");
                // Make it
                var anim = new SEAnim();
                // Add some keys
                anim.AddTranslationKey("shoulder", 0, 0, 0, 0);
                anim.AddTranslationKey("shoulder", 5, 1, 1, 1);
                anim.AddTranslationKey("shoulder", 10, 10, 10, 10);
                anim.AddTranslationKey("shoulder", 30, 20, 20, 20);
                anim.AddTranslationKey("shoulder", 40, 30, 30, 30);
                // Add some scale
                anim.AddScaleKey("shoulder", 0, 1, 1, 1);
                anim.AddScaleKey("shoulder", 50, 3, 3, 3);
                // Add some note
                anim.AddNoteTrack("hello_world", 3);
                anim.AddNoteTrack("bye", 50);

                // Save it
                anim.Write("test3.seanim");
                // Done
                Console.WriteLine("DONE!");
            }

            {
                // Log
                Console.Write("-- Test 4   ");
                // Make it
                var anim = new SEAnim();
                // Add some keys
                anim.AddTranslationKey("shoulder", 0, 0, 0, 0);
                anim.AddTranslationKey("shoulder", 5, 1, 1, 1);
                anim.AddTranslationKey("shoulder", 10, 10, 10, 10);
                anim.AddTranslationKey("shoulder", 30, 20, 20, 20);
                anim.AddTranslationKey("shoulder", 40, 30, 30, 30);
                // Add some scale
                anim.AddScaleKey("shoulder", 0, 1, 1, 1);
                anim.AddScaleKey("shoulder", 50, 3, 3, 3);
                // Add some note
                anim.AddNoteTrack("hello_world", 3);
                anim.AddNoteTrack("bye", 50);

                // Save it (Really, we don't need doubles!!)
                anim.Write("test4.seanim", true);
                // Done
                Console.WriteLine("DONE!");
            }

            {
                // Log
                Console.Write("-- Test 5   ");
                // Make it
                var anim = new SEAnim();
                // Add some keys
                anim.AddTranslationKey("shoulder", 0, 0, 0, 0);
                anim.AddTranslationKey("shoulder", 5, 1, 1, 1);
                anim.AddTranslationKey("shoulder", 10, 10, 10, 10);
                anim.AddTranslationKey("shoulder", 30, 20, 20, 20);
                anim.AddTranslationKey("shoulder", 40, 30, 30, 30);
                // Add some scale
                anim.AddScaleKey("shoulder", 0, 1, 1, 1);
                anim.AddScaleKey("shoulder", 50, 3, 3, 3);
                // Add some rot
                anim.AddRotationKey("shoulder", 0, 0, 0, 0, 1);
                anim.AddRotationKey("shoulder", 50, 0.3, 0.2, 0.5, 1); // Random quat for test
                // Add some note
                anim.AddNoteTrack("hello_world", 3);
                anim.AddNoteTrack("bye", 50);

                // Save it
                anim.Write("test5.seanim");
                // Done
                Console.WriteLine("DONE!");
            }

            #endregion

            // Pause
            Console.Write("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
