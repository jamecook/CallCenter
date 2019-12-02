﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ClientPhoneWebApi.Dto;
using ClientPhoneWebApi.Repo;
using ClientPhoneWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace ClientPhoneWebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ClientController : ControllerBase
    {
        ILogger Logger { get; }
        private RequestService RequestService { get; }
        private static DateTime _lastGetActiveChannelsTime;
        private static ActiveChannelsDto[] _lastActiveChannels;
        public ClientController(RequestService requestService, ILogger<ProductsController> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RequestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _lastGetActiveChannelsTime = _lastGetActiveChannelsTime > DateTime.MinValue
                ? _lastGetActiveChannelsTime
                : DateTime.MinValue;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <response code="200">List of all products</response>
        [HttpGet]
        public Dto.WebUserDto Get([FromQuery]string user,[FromQuery] string password)
        {
            var result = RequestService.WebLogin(user, password);
            return result;
        }
        [HttpGet("activeCalls")]
        public Dto.ActiveChannelsDto[] GetActiveCalls()
        {
            if (_lastGetActiveChannelsTime.AddSeconds(10) < DateTime.Now)
            {
                _lastActiveChannels = RequestService.GetActiveChannels();
                _lastGetActiveChannelsTime = DateTime.Now;
            }

            return _lastActiveChannels;
        }
        /*
        /// <summary>
        /// Get a product by id
        /// </summary>
        /// <param name="id">A product id</param>
        [ProducesResponseType(typeof(Dto.Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("{id}")]
        public Dto.Product GetById(int id)
        {
            return Mapper.Map<Dto.Product>(ProductsRepo.GetById(id));
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="id">A new product id</param>
        /// <param name="newProductDto">New product data</param>
        /// <response code="201">The created product</response>
        [ProducesResponseType(typeof(Dto.Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Route("{id}")]
        public IActionResult Create(int id, [FromBody]Dto.UpdateProduct newProductDto)
        {
            var newProduct = new Model.Product(id);
            Mapper.Map(newProductDto, newProduct);
            ProductsRepo.Create(newProduct);

            var createdProduct = ProductsRepo.GetById(id);

            Logger.LogInformation("New product was created: {@product}", createdProduct);

            return Created($"{id}", Mapper.Map<Dto.Product>(createdProduct));
        }

        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="id">Id of the product to update</param>
        /// <param name="productDto">Product data</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut]
        [Route("{id}")]
        public IActionResult Update(int id, [FromBody]Dto.UpdateProduct productDto)
        {
            var product = ProductsRepo.GetById(id);
            Mapper.Map(productDto, product);
            ProductsRepo.Update(product);
            return Ok();
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">Id of the product to delete</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete(int id)
        {
            ProductsRepo.Delete(id);
            return Ok();
        }

        /// <summary>
        /// Example of an exception handling
        /// </summary>
        [HttpGet("ThrowAnException")]
        public IActionResult ThrowAnException()
        {
            throw new Exception("Example exception");
        }
    /**/
    }

}