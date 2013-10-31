﻿#region License Header
// /*******************************************************************************
//  * Open Behavioral Health Information Technology Architecture (OBHITA.org)
//  * 
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the <organization> nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  * 
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  ******************************************************************************/
#endregion
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Authorization.Mvc;
using Thinktecture.IdentityServer.Models;
using Thinktecture.IdentityServer.Repositories;
using Thinktecture.IdentityServer.Web.Areas.Admin.ViewModels;

namespace Thinktecture.IdentityServer.Web.Areas.Admin.Controllers
{
    [ClaimsAuthorize(Constants.Actions.Administration, Constants.Resources.Configuration)]
    public class IPController : System.Web.Mvc.Controller
    {
        [Import]
        public IIdentityProviderRepository identityProviderRepository { get; set; }

        public IPController()
        {
            Container.Current.SatisfyImportsOnce(this);
        }
        public IPController(IIdentityProviderRepository identityProviderRepository)
        {
            this.identityProviderRepository = identityProviderRepository;
        }

        public ActionResult Index()
        {
            var vm = new IdentityProvidersViewModel(this.identityProviderRepository);
            return View("Index", vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string action, IPModel[] list)
        {
            if (action == "delete") return Delete(list);
            if (action == "new") return Create();
            ModelState.AddModelError("", Resources.IPController.InvalidAction);
            var vm = new IdentityProvidersViewModel(this.identityProviderRepository);
            return View("Index", vm);
        }
        
        [ChildActionOnly]
        public ActionResult Menu()
        {
            var list = new IdentityProvidersViewModel(this.identityProviderRepository);
            if (list.IdentityProviders.Any())
            {
                var vm = new ChildMenuViewModel
                {
                    Items = list.IdentityProviders.Select(x =>
                        new ChildMenuItem
                        {
                            Controller = "IP",
                            Action = "IP",
                            Title = x.Name,
                            RouteValues = new { id = x.ID }
                        }).ToArray()
                };
                return PartialView("ChildMenu", vm);
            }
            return new EmptyResult();
        }

        ActionResult Delete(IPModel[] list)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (list != null)
                    {
                        foreach (var item in list.Where(x => x.Delete))
                        {
                            this.identityProviderRepository.Delete(item.ID);
                        }
                        TempData["Message"] = Resources.IPController.IdentityProvidersDeleted;
                    }
                    return RedirectToAction("Index");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", Resources.IPController.ErrorDeletingIdentityProviders);
                }
            }
            var vm = new IdentityProvidersViewModel(this.identityProviderRepository);
            return View("Index", vm);
        }

        private ActionResult Create()
        {
            return View("IP", new IdentityProvider());
        }

        public ActionResult IP(int id)
        {
            var ip = this.identityProviderRepository.Get(id);
            if (ip == null) return HttpNotFound();
            return View("IP", ip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IdentityProvider model, IPCertInputModel cert)
        {
            if (cert != null && cert.Cert != null)
            {
                model.IssuerThumbprint = cert.Cert.Thumbprint;
                if (model.IssuerThumbprint != null)
                {
                    ModelState["IssuerThumbprint"].Errors.Clear();
                    ModelState["IssuerThumbprint"].Value = new ValueProviderResult(model.IssuerThumbprint, model.IssuerThumbprint, ModelState["IssuerThumbprint"].Value.Culture);
                }
            } 

            if (ModelState.IsValid)
            {
                try
                {
                    this.identityProviderRepository.Add(model);
                    TempData["Message"] = Resources.IPController.IdentityProviderCreated;
                    return RedirectToAction("IP", new { id=model.ID });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", Resources.IPController.ErrorCreatingIdentityProvider);
                }
            }
            
            // if we're here, then we should clear name so the view thinks it's new
            model.ID = 0;
            return View("IP", model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(IdentityProvider model, IPCertInputModel cert, string action)
        {
            if (action == "delete")
            {
                this.identityProviderRepository.Delete(model.ID);
                TempData["Message"] = Resources.IPController.IdentityProvidersDeleted;
                return RedirectToAction("Index");
            }

            if (cert != null && cert.Cert != null)
            {
                model.IssuerThumbprint = cert.Cert.Thumbprint;
                ModelState["IssuerThumbprint"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this.identityProviderRepository.Update(model);
                    TempData["Message"] = Resources.IPController.IdentityProviderUpdated; ;
                    return RedirectToAction("IP", new { id = model.ID });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch
                {
                    ModelState.AddModelError("", Resources.IPController.ErrorUpdatingIdentityProvider);
                }
            }
            
            return View("IP", model);
        }
    }
}
