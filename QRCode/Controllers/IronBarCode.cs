using Microsoft.AspNetCore.Mvc;
using IronBarCode;
using Microsoft.Extensions.Hosting;
using System.Drawing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing;

namespace QRCode.Controllers
{
    public class IronBarCode : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        public IronBarCode(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ReadBarCode(IFormFile QRCodeImage)
        {
            if (QRCodeImage == null || QRCodeImage.Length == 0)
            {
                ViewBag.BarcodeText = "No image uploaded";
                ViewBag.QRCodeText = "No image uploaded";
                return View("Index");
            }
            using (var stream = new MemoryStream())
            {
                    QRCodeImage.CopyTo(stream);
                    stream.Position = 0;
                BarcodeReaderOptions myOptionsExample = new BarcodeReaderOptions()
                {
                    ExpectBarcodeTypes = BarcodeEncoding.QRCode | BarcodeEncoding.Code128,
                    
                };

                using (var image = Image.FromStream(stream))
                {
                    var results = BarcodeReader.Read(image, myOptionsExample);
                    if (results != null)
                    {
                        foreach(BarcodeResult result in results)
                        {
                            string value = result.Value;
                            if(result.BarcodeType == BarcodeEncoding.Code128)
                            {
                                ViewBag.BarcodeText = value;
                            }else if(result.BarcodeType == BarcodeEncoding.QRCode)
                            {
                                ViewBag.QrcodeText = value;
                            }
                        }
                    }
                }
            }
            return View("Index");
        }

    }
}
