using System;

namespace UCLouvain.BDDSharp.Table
{
	public class BDDNodeBucketException : Exception
	{
		public BDDNodeBucketException()
		{
		}

		public BDDNodeBucketException(string message) : base(message)
		{
		}
	}
}