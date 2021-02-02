using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using OCLink.Models;
using ZMLib;

namespace OCLink
{
    public class Commountity
    {
		public static HISEntities CreateDBContext()
		{
			ZMClass myClass = new ZMClass();

			//加解密的class			
			//MyEncrypt myEncrypt = new MyEncrypt();

			//從App.Config(or Web.Config)取出的設定加密字串，並解密
			String db_DataSource = myClass.AESDecrypt(ConfigurationManager.AppSettings["db_DataSource"], "z1r@m1d!@#$%^&*(");
			String db_Catalog = myClass.AESDecrypt(ConfigurationManager.AppSettings["db_Catalog"], "z1r@m1d!@#$%^&*(");
			String db_User = myClass.AESDecrypt(ConfigurationManager.AppSettings["db_User"], "z1r@m1d!@#$%^&*(");
			String db_Pwd = myClass.AESDecrypt(ConfigurationManager.AppSettings["db_Pwd"], "z1r@m1d!@#$%^&*(");

			string db_ConnectionStr = string.Format(ConfigurationManager.AppSettings["db_ConnectionStr"], db_DataSource, db_Catalog, db_User, db_Pwd);

			//回傳上面我自訂的ContosoUniversityEntities Constructor(有參數)
			return new HISEntities(db_ConnectionStr);
		}
	}
}
