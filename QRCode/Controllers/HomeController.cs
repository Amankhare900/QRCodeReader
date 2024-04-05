using Microsoft.AspNetCore.Mvc;
using QRCode.Models;
using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
using QRCode.Models;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Reflection.PortableExecutable;

namespace QRCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(IFormCollection formCollection)
        {
            // Generate QR code
            var qrWriter = new QRCodeWriter();
            var qrResultBit = qrWriter.encode(formCollection["QRCodeString"], BarcodeFormat.QR_CODE, 200, 200);
            var qrMatrix = qrResultBit;
            Bitmap qrCodeBitmap = GenerateBitmap(qrMatrix);

            // Generate barcode
            var barcodeWriter = new BarcodeWriter()
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 100,
                    Width = 300
                }
            };
            var barcodeResult = barcodeWriter.Write(formCollection["QRCodeString"]);
            Bitmap barcodeBitmap = barcodeResult;

            // Save generated images
            string webRootPath = _hostEnvironment.WebRootPath;
            qrCodeBitmap.Save(webRootPath + "\\Images\\QrcodeNew.png");
            barcodeBitmap.Save(webRootPath + "\\Images\\BarcodeNew.png");

            // Pass URLs to the view
            ViewBag.QRCodeUrl = "\\Images\\QrcodeNew.png";
            ViewBag.BarcodeUrl = "\\Images\\BarcodeNew.png";

            return View();
        }

        // Helper method to generate bitmap from BitMatrix
        private Bitmap GenerateBitmap(BitMatrix matrix, int scale = 2)
        {
            Bitmap result = new Bitmap(matrix.Width * scale, matrix.Height * scale);
            for (int x = 0; x < matrix.Height; x++)
            {
                for (int y = 0; y < matrix.Width; y++)
                {
                    Color pixel = matrix[x, y] ? Color.Black : Color.White;
                    for (int i = 0; i < scale; i++)
                        for (int j = 0; j < scale; j++)
                        {
                            result.SetPixel(x * scale + i, y * scale + j, pixel);
                        }
                }
            }
            return result;
        }

        [HttpGet]
        public IActionResult ReadQRCode()
        {
            string webRootPath = _hostEnvironment.WebRootPath;
            var path = webRootPath + "\\Images\\QrcodeNew4.png";

            var reader = new BarcodeReaderGeneric()
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_39, }
                }
            };
            var QrReader = new QRCodeReader();

            Bitmap image = (Bitmap)Image.FromFile(path);
            using (image)
            {
                LuminanceSource source = new ZXing.Windows.Compatibility.BitmapLuminanceSource(image);
                BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));

                Result[] barcodeResult = reader.DecodeMultiple(source);
                Result qrCodeResult = QrReader.decode(bitmap);

                if (barcodeResult != null)
                {
                    foreach (var code in barcodeResult)
                    {
                        if (code.BarcodeFormat == ZXing.BarcodeFormat.CODE_128)
                        {
                            ViewBag.BarcodeText = barcodeResult[^1].Text;
                            break;
                        }
                    }
                }
                else
                {
                    ViewBag.BarcodeText = "Not able to read barcode";
                }

                if (qrCodeResult != null)
                {
                    ViewBag.QRCodeText = qrCodeResult.Text;
                }
                else
                {
                    ViewBag.QRCodeText = "Not able to read QR code";
                }
            }

            return View("Index");
        }

        [HttpPost]
        public IActionResult ReadQRCode(IFormFile QRCodeImage)
        {
            if (QRCodeImage == null || QRCodeImage.Length == 0)
            {
                ViewBag.BarcodeText = "No image uploaded";
                ViewBag.QRCodeText = "No image uploaded";
                return View("Index");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    QRCodeImage.CopyTo(stream);
                    stream.Position = 0;

                    var reader = new BarcodeReaderGeneric()
                    {
                        AutoRotate = true,
                        Options = new DecodingOptions
                        {
                            TryHarder = true
                        }
                    };
                    var QrReader = new QRCodeReader();

                    using (var image = Image.FromStream(stream))
                    {
                        LuminanceSource source = new ZXing.Windows.Compatibility.BitmapLuminanceSource((Bitmap)image);
                        BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));

                        Result[] barcodeResult = reader.DecodeMultiple(source);
                        Result qrCodeResult = QrReader.decode(bitmap);

                        if (barcodeResult != null)
                        {
                            foreach (var code in barcodeResult)
                            {
                                if (code.BarcodeFormat == ZXing.BarcodeFormat.CODE_128)
                                {
                                    ViewBag.BarcodeText = barcodeResult[barcodeResult.Length - 1].Text;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ViewBag.BarcodeText = "Not able to read barcode";
                        }

                        if (qrCodeResult != null)
                        {
                            ViewBag.QRCodeText = qrCodeResult.Text;
                        }
                        else
                        {
                            ViewBag.QRCodeText = "Not able to read QR code";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.BarcodeText = "Error: " + ex.Message;
                ViewBag.QRCodeText = "Error: " + ex.Message;
            }

            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}