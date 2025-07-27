// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("a2P3bh8XTlq7OC32OYV3lhjtqVJzupSotIpvSAaN2wLHkiwwNhAVUcoIkJCNpilucqxXNyPB8rSztyr3OtCcdnLSK+g6EZ840XH3TjCLGgqleHSxyIthGhidB17qhNfHB0oxnXxYY2067oOPy392QV8oqp7nNeTccp7JDX/XWN0boKfTOzTsfPwyqq5eq6ygQjrFN60tJ8XCc3PNoXNhxdQ21QEvzUjf5i5r9yfOVUAbWTWL6NzFTw6mHgT3LOVkmD0VcbZ9JLtc7m1OXGFqZUbqJOqbYW1tbWlsb4mrdpHkzW0CtvFC2UGJV4vcxIHr7m1jbFzubWZu7m1tbLAdC8lZmm0YmzpkztNMKQf7Og38x3bREL0nswbrSMazgRn7sW5vbWxt");
        private static int[] order = new int[] { 8,9,12,5,13,5,11,12,12,11,12,13,13,13,14 };
        private static int key = 108;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
