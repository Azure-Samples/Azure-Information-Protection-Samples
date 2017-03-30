using System;
using System.Threading.Tasks;

namespace com.microsoft.rightsmanagement.windows.viewer.Models
{
	internal class SignInModel
	{
		public Func<bool> OnSignIn;
		public Action OnSignUp;
		public Action OnFinish;
	}
}
