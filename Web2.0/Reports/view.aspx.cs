/**********************************************************************************************************************
 * The contents of this file are subject to the SugarCRM Public License Version 1.1.3 ("License"); You may not use this
 * file except in compliance with the License. You may obtain a copy of the License at http://www.sugarcrm.com/SPL
 * Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 * express or implied.  See the License for the specific language governing rights and limitations under the License.
 *
 * All copies of the Covered Code must include on each user interface screen:
 *    (i) the "Powered by SugarCRM" logo and
 *    (ii) the SugarCRM copyright notice
 *    (iii) the SplendidCRM copyright notice
 * in the same form as they appear in the distribution.  See full license for requirements.
 *
 * The Original Code is: SplendidCRM Open Source
 * The Initial Developer of the Original Code is SplendidCRM Software, Inc.
 * Portions created by SplendidCRM Software are Copyright (C) 2005 SplendidCRM Software, Inc. All Rights Reserved.
 * Contributor(s): ______________________________________.
 *********************************************************************************************************************/
using System;
using System.Data;
using System.Data.Common;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics;
//using Microsoft.VisualBasic;

namespace SplendidCRM.Reports
{
	/// <summary>
	/// Summary description for View.
	/// </summary>
	public class View : SplendidPage
	{
		protected SplendidCRM._controls.ModuleHeader ctlModuleHeader;
		protected ReportView ctlReportView;

		protected RdlDocument     rdl = null;
		protected Guid            gID       ;
		protected Label           lblError  ;

		private void Page_Load(object sender, System.EventArgs e)
		{
			try
			{
				rdl = new RdlDocument();
				if ( !IsPostBack )
				{
					Page.DataBind();
					gID = Sql.ToGuid(Request["ID"]);
					if ( !Sql.IsEmptyGuid(gID) )
					{
						DbProviderFactory dbf = DbProviderFactories.GetFactory();
						using ( IDbConnection con = dbf.CreateConnection() )
						{
							string sSQL;
							sSQL = "select *             " + ControlChars.CrLf
							     + "  from vwREPORTS_Edit" + ControlChars.CrLf
							     + " where ID = @ID      " + ControlChars.CrLf;
							using ( IDbCommand cmd = con.CreateCommand() )
							{
								cmd.CommandText = sSQL;
								Sql.AddParameter(cmd, "@ID", gID);
								con.Open();

								using ( IDataReader rdr = cmd.ExecuteReader(CommandBehavior.SingleRow) )
								{
									if ( rdr.Read() )
									{
										ctlModuleHeader.Title = Sql.ToString(rdr["NAME"]);
										SetPageTitle(L10n.Term(".moduleList.Reports") + " - " + ctlModuleHeader.Title);
										ViewState["ctlModuleHeader.Title"] = ctlModuleHeader.Title;

										string sXML = Sql.ToString(rdr["RDL"]);
										try
										{
											if ( !Sql.IsEmptyString(sXML) )
											{
												rdl.LoadRdl(Sql.ToString(rdr["RDL"]));
												ctlReportView.RunReport(rdl.OuterXml);
											}
										}
										catch
										{
										}
									}
								}
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				SplendidError.SystemError(new StackTrace(true).GetFrame(0), ex);
				lblError.Text = ex.Message;
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
