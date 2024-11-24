using Compiler.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Tokenization
{
    class TokenizerTest
    {
        static void Main(string[] args)
        {
            List<string> testInputs = new List<string>
        {
            "$abc$", "abc123", "123abc", "1,234", "1,,234",
            "'a'", "'ab'", "+", ":", ":=", "!", "if x > 0"
        };

            Tokenizer tokenizer = new Tokenizer(new StringReader(string.Join(" ", testInputs)), new ErrorReporter());

            Console.WriteLine("Testing Tokenizer:");
            List<Token> tokens = tokenizer.GetAllTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }
    }

}
