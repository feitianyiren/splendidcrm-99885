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
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics;
//using Microsoft.VisualBasic;

namespace SplendidCRM.Contacts
{
	/// <summary>
	///		Summary description for Activities.
	/// </summary>
	public class Activities : SplendidControl
	{
		protected Guid          gID            ;
		protected DataView      vwOpen         ;
		protected SplendidGrid  grdOpen        ;
		protected DataView      vwHistory      ;
		protected SplendidGrid  grdHistory     ;
		protected Label         lblError       ;

		protected void Page_Command(object sender, CommandEventArgs e)
		{
			try
			{
				switch ( e.CommandName )
				{
					case "Tasks.Create":
						Response.Redirect("~/Tasks/edit.aspx?PARENT_ID=" + gID.ToString());
						break;
					case "Meetings.Create":
						Response.Redirect("~/Meetings/edit.aspx?PARENT_ID=" + gID.ToString());
						break;
					case "Calls.Create":
						Response.Redirect("~/Calls/edit.aspx?PARENT_ID=" + gID.ToString());
						break;
					case "Emails.Compose":
						Response.Redirect("~/Emails/edit.aspx?PARENT_ID=" + gID.ToString());
						break;
					case "Notes.Create":
						Response.Redirect("~/Notes/edit.aspx?PARENT_ID=" + gID.ToString());
						break;
					case "Emails.Archive":
						Response.Redirect("~/Emails/edit.aspx?PARENT_ID=" + gID.ToString());
						break;
					case "Activities.Delete":
					{
						Guid gACTIVITY_ID = Sql.ToGuid(e.CommandArgument);
						SqlProcs.spACTIVITIES_Delete(gACTIVITY_ID);
						// 08/30/2006 Paul.  We need to redirect so that the activities list will reflect the deleted item. 
						Response.Redirect("view.aspx?ID=" + gID.ToString());
						break;
					}
					default:
						throw(new Exception("Unknown command: " + e.CommandName));
				}
			}
			catch(Exception ex)
			{
				SplendidError.SystemError(new StackTrace(true).GetFrame(0), ex.Message);
				lblError.Text = ex.Message;
			}
		}

		private void Page_Load(object sender, System.EventArgs e)
		{
			gID = Sql.ToGuid(Request["ID"]);

			DbProviderFactory dbf = DbProviderFactories.GetFactory();
			using ( IDbConnection con = dbf.CreateConnection() )
			{
				string sSQL;
				// 08/07/2006 Paul.  A contact can have two relationships with a single note, show only one. 
				sSQL = "select distinct *                " + ControlChars.CrLf
				     + "  from vwCONTACTS_ACTIVITIES     " + ControlChars.CrLf
				     + " where CONTACT_ID = @CONTACT_ID  " + ControlChars.CrLf
				     + " order by DATE_DUE desc          " + ControlChars.CrLf;
				using ( IDbCommand cmd = con.CreateCommand() )
				{
					cmd.CommandText = sSQL;
					Sql.AddParameter(cmd, "@CONTACT_ID", gID);
#if DEBUG
					Page.RegisterClientScriptBlock("vwCONTACTS_ACTIVITIES", Sql.ClientScriptBlock(cmd));
#endif
					try
					{
						using ( DbDataAdapter da = dbf.CreateDataAdapter() )
						{
							((IDbDataAdapter)da).SelectCommand = cmd;
							using ( DataTable dt = new DataTable() )
							{
								da.Fill(dt);
								// 11/26/2005 Paul.  Convert the term here so that sorting will apply. 
								foreach(DataRow row in dt.Rows)
								{
									// 11/26/2005 Paul.  Status is translated differently for each type. 
									switch ( Sql.ToString(row["ACTIVITY_TYPE"]) )
									{
										// 07/15/2006 Paul.  Translation of Call status remains here because it is more complex than the standard list translation. 
										case "Calls"   :  row["STATUS"] = L10n.Term(".call_direction_dom.", row["DIRECTION"]) + " " + L10n.Term(".call_status_dom.", row["STATUS"]);  break;
										//case "Meetings":  row["STATUS"] = L10n.Term("Meeting") + " " + L10n.Term(".meeting_status_dom.", row["STATUS"]);  break;
										//case "Tasks"   :  row["STATUS"] = L10n.Term("Task"   ) + " " + L10n.Term(".task_status_dom."   , row["STATUS"]);  break;
									}
								}
								vwOpen = new DataView(dt);
								vwOpen.RowFilter    = "IS_OPEN = 1";
								grdOpen.DataSource    = vwOpen ;

								vwHistory = new DataView(dt);
								vwHistory.RowFilter = "IS_OPEN = 0";
								grdHistory.DataSource = vwHistory ;
								// 09/05/2005 Paul. LinkButton controls will not fire an event unless the the grid is bound. 
								//if ( !IsPostBack )
								{
									grdOpen.SortColumn = "DATE_DUE";
									grdOpen.SortOrder  = "desc" ;
									grdOpen.ApplySort();
									grdOpen.DataBind();
									grdHistory.SortColumn = "DATE_MODIFIED";
									grdHistory.SortOrder  = "desc" ;
									grdHistory.ApplySort();
									grdHistory.DataBind();
								}
							}
						}
					}
					catch(Exception ex)
					{
						SplendidError.SystemError(new StackTrace(true).GetFrame(0), ex.Message);
						lblError.Text = ex.Message;
					}
				}
			}
			if ( !IsPostBack )
			{
				// 06/09/2006 Paul.  Remove data binding in the user controls.  Binding is required, but only do so in the ASPX pages. 
				//Page.DataBind();
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);
			// 11/26/2005 Paul.  Add fields early so that sort events will get called. 
			grdOpen   .DynamicColumns("Contacts.Activities.Open"   );
			grdHistory.DynamicColumns("Contacts.Activities.History");
		}
		#endregion
	}
}
