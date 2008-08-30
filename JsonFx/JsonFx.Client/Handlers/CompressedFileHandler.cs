using System;

namespace JsonFx.Handlers
{
	public class CompressedFileHandler : System.Web.DefaultHttpHandler
	{
		public override IAsyncResult BeginProcessRequest(System.Web.HttpContext context, AsyncCallback callback, object state)
		{
			CompiledBuildResult.EnableStreamCompression(context);

			return base.BeginProcessRequest(context, callback, state);
		}
	}
}
