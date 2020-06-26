using redHut.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Data.Entity.Migrations;

namespace redHut.Controllers
{
    public class HomeController : Controller
    {
        private userAccountDBContext db = new userAccountDBContext();

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(UserAccountModel model, bool? value)
        {
            if (model != null)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(model.Password + model.EmailId);
                Byte[] encodedBytes = md5.ComputeHash(originalBytes);

                string hashedPassword = BitConverter.ToString(encodedBytes).Replace("-", "");


                var nouser = db.userAccountModel.Where(u => u.EmailId == model.EmailId && u.Password != hashedPassword).Any();
                var newudb = db.userAccountModel.Where(u => u.EmailId == model.EmailId && u.Password == hashedPassword).FirstOrDefault();
                if (newudb != null)
                {
                    Session["ID"] = newudb.ID.ToString();
                    Session["UserName"] = newudb.UserName.ToString();
                    return RedirectToAction("IndexList", "Home", model);
                }
                else if (nouser == true)
                {
                    ModelState.AddModelError("", "Password doesnot match with email-id");
                }
                else
                {
                    ModelState.AddModelError("", "credentials mis-match");
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult IndexList()
        {
            if (Session["UserName"] != null)
            {
                var checkList = db.userAccountModel.OrderBy(x => x.FirstName).ToList();
                return View(checkList);
            }
            else if (Session["UserName"] == null)
            {
                return RedirectToAction("Signin", "Home");
            }
            return View();
        }

        //REGISTER
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserAccountModel model)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState != null)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(model.Password + model.EmailId);
                Byte[] encodedBytes = md5.ComputeHash(originalBytes);

                string hashedPassword = BitConverter.ToString(encodedBytes).Replace("-", "");

                model.Password = hashedPassword;
                model.ConfirmPassword = hashedPassword;
                db.userAccountModel.Add(model);
                db.SaveChanges();

                ModelState.Clear();
                TempData["Success"] = model.FirstName + " " + model.LastName + " successfully registered.";
                ModelState.Clear();
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        //LOGIN
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserAccountModel model, bool? value)
        {
            if (model != null)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(model.Password + model.EmailId);
                Byte[] encodedBytes = md5.ComputeHash(originalBytes);

                string hashedPassword = BitConverter.ToString(encodedBytes).Replace("-", "");

                var nouser = db.userAccountModel.Where(u => u.EmailId == model.EmailId && u.Password != hashedPassword).Any();
                var newudb = db.userAccountModel.Where(u => u.EmailId == model.EmailId && u.Password == hashedPassword).FirstOrDefault();
                if (newudb != null)
                {
                    Session["ID"] = newudb.ID.ToString();
                    Session["UserName"] = newudb.UserName.ToString();
                    return RedirectToAction("IndexList", "Home");
                }
                else if (nouser == true)
                {
                    ModelState.AddModelError("", "Password doesnot match with email-id");
                }
                else
                {
                    ModelState.AddModelError("", "credentials mis-match");
                }
            }
            return View();
        }


        //LOGOUT
        public ActionResult Logout()
        {
            {
                FormsAuthentication.SignOut();

                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Home");

            }
        }


        //DELETE
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccountModel user = db.userAccountModel.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {

            UserAccountModel user = db.userAccountModel.Find(id);
            db.userAccountModel.Remove(user);

            db.SaveChanges();
            if (user.ID.ToString() == Session["ID"].ToString())
            {
                Session.Abandon();
            }

            return RedirectToAction("Index", "Home");
        }



        //EDIT
        public ActionResult Edit(int? id, UserAccountModel adm)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (Session["ID"].ToString() != adm.ID.ToString())
            {
                ModelState.AddModelError("", "you are not authorized to make data changes except your own.!");
                return RedirectToAction("Index", "Home", adm);
            }
            else
            {
                //           var model = new UserAccountViewModel();
                UserAccountModel user = db.userAccountModel.Find(id);

                return View(user);
            }
        }



        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UserAccountModel user)
        {
            try
            {
                var newudb = db.userAccountModel.Where(u => u.EmailId == user.EmailId && u.ID == user.ID).First();

                Session["ID"] = newudb.ID.ToString();
                Session["UserName"] = newudb.UserName.ToString();
                user.Password = user.Password;
                user.ConfirmPassword = user.ConfirmPassword;
                //db.Entry(user).State = EntityState.Modified;
                db.Set<UserAccountModel>().AddOrUpdate(user);


                db.SaveChanges();
                Session["UserName"] = newudb.UserName.ToString();
                ModelState.Clear();
                TempData["updated"] = user.FirstName + " " + user.LastName + " successfully updated";
            }
            catch (Exception ex)
            { }

            return RedirectToAction("IndexList", "Home", user);
          }

    }
}
