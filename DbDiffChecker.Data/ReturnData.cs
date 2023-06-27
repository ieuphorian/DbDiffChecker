using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbDiffChecker.Data
{
	public class ReturnData<T>
	{
		/// <summary>
		/// Is successful
		/// </summary>
		public bool IsSucccesful { get; set; } = true;

		/// <summary>
		/// Error Message
		/// </summary>
		public string ErrorMessage { get; set; } = string.Empty;

		/// <summary>
		/// Return Data
		/// </summary>
		public T Data { get; set; }

        /// <summary>
        /// Additional String Data
        /// </summary>
        public string AdditionalData { get; set; }
	}
}
