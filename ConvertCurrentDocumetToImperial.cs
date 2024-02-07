using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace UnitsConverter
{
    [Transaction(TransactionMode.Manual)]
    internal class ConvertCurrentDocumetToImperial : IExternalCommand
    {
        //static AddInId appId = new AddInId(new Guid("2554BB9D-7D57-4FB2-B706-DEE97AED7A29"));

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            // the code get the document
            var doc = commandData.Application.ActiveUIDocument.Document;

            //UIDocument uidoc = this.ActiveUIDocument;
            //Document doc = uidoc.Document;

            //get the units in the document
            var units = doc.GetUnits();

            //UTLength
            var foUTLength = units.GetFormatOptions(SpecTypeId.Length);
            foUTLength.Accuracy = 0.00260416666666667;
            foUTLength.SetUnitTypeId(UnitTypeId.FractionalInches);
            units.SetFormatOptions(SpecTypeId.Length, foUTLength);
            //UTArea
            var foUTArea = units.GetFormatOptions(SpecTypeId.Area);
            foUTArea.Accuracy = 0.01;
            foUTArea.SetUnitTypeId(UnitTypeId.SquareFeet);
            units.SetFormatOptions(SpecTypeId.Area, foUTArea);
            //UTVolume
            var foUTVolume = units.GetFormatOptions(SpecTypeId.Volume);
            foUTVolume.Accuracy = 0.01;
            foUTVolume.SetUnitTypeId(UnitTypeId.CubicFeet);
            units.SetFormatOptions(SpecTypeId.Volume, foUTVolume);
            //UTAngle
            var foUTAngle = units.GetFormatOptions(SpecTypeId.Angle);
            foUTAngle.Accuracy = 0.01;
            foUTAngle.SetUnitTypeId(UnitTypeId.Degrees);
            units.SetFormatOptions(SpecTypeId.Angle, foUTAngle);
            //UTHVACDensity
            var foUTHVACDensity = units.GetFormatOptions(SpecTypeId.HvacDensity);
            foUTHVACDensity.Accuracy = 0.0001;
            foUTHVACDensity.SetUnitTypeId(UnitTypeId.PoundsMassPerCubicFoot);
            units.SetFormatOptions(SpecTypeId.HvacDensity, foUTHVACDensity);


            //UTHVACEnergy
            var foUTHVACEnergy = units.GetFormatOptions(SpecTypeId.HvacEnergy);
            foUTHVACEnergy.Accuracy = 1;
            foUTHVACEnergy.SetUnitTypeId(UnitTypeId.BritishThermalUnits);
            units.SetFormatOptions(SpecTypeId.HvacEnergy, foUTHVACEnergy);
            //UTHVACFriction
            var foUTHVACFriction = units.GetFormatOptions(SpecTypeId.HvacFriction);
            foUTHVACFriction.Accuracy = 0.01;
            foUTHVACFriction.SetUnitTypeId(UnitTypeId.InchesOfWater60DegreesFahrenheitPer100Feet);
            units.SetFormatOptions(SpecTypeId.HvacFriction, foUTHVACFriction);
            //UTHVACPower
            var foUTHVACPower = units.GetFormatOptions(SpecTypeId.HvacPower);
            foUTHVACPower.Accuracy = 1;
            foUTHVACPower.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.HvacPower, foUTHVACPower);
            //UTHVACPowerDensity
            var foUTHVACPowerDensity = units.GetFormatOptions(SpecTypeId.HvacPowerDensity);
            foUTHVACPowerDensity.Accuracy = 0.01;
            foUTHVACPowerDensity.SetUnitTypeId(UnitTypeId.WattsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.HvacPowerDensity, foUTHVACPowerDensity);
            //UTHVACPressure
            var foUTHVACPressure = units.GetFormatOptions(SpecTypeId.HvacPressure);
            foUTHVACPressure.Accuracy = 0.01;
            foUTHVACPressure.SetUnitTypeId(UnitTypeId.InchesOfWater60DegreesFahrenheit);
            units.SetFormatOptions(SpecTypeId.HvacPressure, foUTHVACPressure);
            //UTHVACTemperature
            var foUTHVACTemperature = units.GetFormatOptions(SpecTypeId.HvacTemperature);
            foUTHVACTemperature.Accuracy = 1;
            foUTHVACTemperature.SetUnitTypeId(UnitTypeId.Fahrenheit);
            units.SetFormatOptions(SpecTypeId.HvacTemperature, foUTHVACTemperature);


            //UTHVACVelocity
            var foUTHVACVelocity = units.GetFormatOptions(SpecTypeId.HvacVelocity);
            foUTHVACVelocity.Accuracy = 1;
            foUTHVACVelocity.SetUnitTypeId(UnitTypeId.FeetPerMinute);
            units.SetFormatOptions(SpecTypeId.HvacVelocity, foUTHVACVelocity);
            //UTHVACAirFlow
            var foUTHVACAirFlow = units.GetFormatOptions(SpecTypeId.AirFlow);
            foUTHVACAirFlow.Accuracy = 1;
            foUTHVACAirFlow.SetUnitTypeId(UnitTypeId.CubicFeetPerMinute);
            units.SetFormatOptions(SpecTypeId.AirFlow, foUTHVACAirFlow);
            //UTHVACDuctSize
            var foUTHVACDuctSize = units.GetFormatOptions(SpecTypeId.DuctSize);
            foUTHVACDuctSize.Accuracy = 1;
            foUTHVACDuctSize.SetUnitTypeId(UnitTypeId.FractionalInches);
            units.SetFormatOptions(SpecTypeId.DuctSize, foUTHVACDuctSize);
            //UTHVACCrossSection
            var foUTHVACCrossSection = units.GetFormatOptions(SpecTypeId.CrossSection);
            foUTHVACCrossSection.Accuracy = 0.01;
            foUTHVACCrossSection.SetUnitTypeId(UnitTypeId.SquareInches);
            units.SetFormatOptions(SpecTypeId.CrossSection, foUTHVACCrossSection);
            //UTHVACHeatGain
            var foUTHVACHeatGain = units.GetFormatOptions(SpecTypeId.HeatGain);
            foUTHVACHeatGain.Accuracy = 0.1;
            foUTHVACHeatGain.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.HeatGain, foUTHVACHeatGain);
            //UTElectricalCurrent
            var foUTElectricalCurrent = units.GetFormatOptions(SpecTypeId.Current);
            foUTElectricalCurrent.Accuracy = 1;
            foUTElectricalCurrent.SetUnitTypeId(UnitTypeId.Amperes);
            units.SetFormatOptions(SpecTypeId.Current, foUTElectricalCurrent);
            //UTElectricalPotential
            var foUTElectricalPotential = units.GetFormatOptions(SpecTypeId.ElectricalPotential);
            foUTElectricalPotential.Accuracy = 1;
            foUTElectricalPotential.SetUnitTypeId(UnitTypeId.Volts);
            units.SetFormatOptions(SpecTypeId.ElectricalPotential, foUTElectricalPotential);
            //UTElectricalFrequency
            var foUTElectricalFrequency = units.GetFormatOptions(SpecTypeId.ElectricalFrequency);
            foUTElectricalFrequency.Accuracy = 1;
            foUTElectricalFrequency.SetUnitTypeId(UnitTypeId.Hertz);
            units.SetFormatOptions(SpecTypeId.ElectricalFrequency, foUTElectricalFrequency);
            //UTElectricalIlluminance
            var foUTElectricalIlluminance = units.GetFormatOptions(SpecTypeId.Illuminance);
            foUTElectricalIlluminance.Accuracy = 1;
            foUTElectricalIlluminance.SetUnitTypeId(UnitTypeId.Lux);
            units.SetFormatOptions(SpecTypeId.Illuminance, foUTElectricalIlluminance);


            //UTElectricalLuminance
            var foUTElectricalLuminance = units.GetFormatOptions(SpecTypeId.Luminance);
            foUTElectricalLuminance.Accuracy = 1;
            foUTElectricalLuminance.SetUnitTypeId(UnitTypeId.CandelasPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.Luminance, foUTElectricalLuminance);
            //UTElectricalLuminousFlux
            var foUTElectricalLuminousFlux = units.GetFormatOptions(SpecTypeId.LuminousFlux);
            foUTElectricalLuminousFlux.Accuracy = 1;
            foUTElectricalLuminousFlux.SetUnitTypeId(UnitTypeId.Lumens);
            units.SetFormatOptions(SpecTypeId.LuminousFlux, foUTElectricalLuminousFlux);
            //UTElectricalLuminousIntensity
            var foUTElectricalLuminousIntensity = units.GetFormatOptions(SpecTypeId.LuminousIntensity);
            foUTElectricalLuminousIntensity.Accuracy = 1;
            foUTElectricalLuminousIntensity.SetUnitTypeId(UnitTypeId.Candelas);
            units.SetFormatOptions(SpecTypeId.LuminousIntensity, foUTElectricalLuminousIntensity);
            //UTElectricalEfficacy
            var foUTElectricalEfficacy = units.GetFormatOptions(SpecTypeId.Efficacy);
            foUTElectricalEfficacy.Accuracy = 1;
            foUTElectricalEfficacy.SetUnitTypeId(UnitTypeId.LumensPerWatt);
            units.SetFormatOptions(SpecTypeId.Efficacy, foUTElectricalEfficacy);
            //UTElectricalWattage
            var foUTElectricalWattage = units.GetFormatOptions(SpecTypeId.Wattage);
            foUTElectricalWattage.Accuracy = 1;
            foUTElectricalWattage.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.Wattage, foUTElectricalWattage);
            //UTColorTemperature
            var foUTColorTemperature = units.GetFormatOptions(SpecTypeId.ColorTemperature);
            foUTColorTemperature.Accuracy = 1;
            foUTColorTemperature.SetUnitTypeId(UnitTypeId.Kelvin);
            units.SetFormatOptions(SpecTypeId.ColorTemperature, foUTColorTemperature);
            //UTElectricalPower
            var foUTElectricalPower = units.GetFormatOptions(SpecTypeId.ElectricalPower);
            foUTElectricalPower.Accuracy = 1;
            foUTElectricalPower.SetUnitTypeId(UnitTypeId.Watts);
            units.SetFormatOptions(SpecTypeId.ElectricalPower, foUTElectricalPower);


            //UTHVACRoughness
            var foUTHVACRoughness = units.GetFormatOptions(SpecTypeId.HvacRoughness);
            foUTHVACRoughness.Accuracy = 0.0001;
            foUTHVACRoughness.SetUnitTypeId(UnitTypeId.Feet);
            units.SetFormatOptions(SpecTypeId.HvacRoughness, foUTHVACRoughness);
            //UTElectricalApparentPower
            var foUTElectricalApparentPower = units.GetFormatOptions(SpecTypeId.ApparentPower);
            foUTElectricalApparentPower.Accuracy = 1;
            foUTElectricalApparentPower.SetUnitTypeId(UnitTypeId.VoltAmperes);
            units.SetFormatOptions(SpecTypeId.ApparentPower, foUTElectricalApparentPower);
            //UTElectricalPowerDensity
            var foUTElectricalPowerDensity = units.GetFormatOptions(SpecTypeId.ElectricalPowerDensity);
            foUTElectricalPowerDensity.Accuracy = 0.01;
            foUTElectricalPowerDensity.SetUnitTypeId(UnitTypeId.WattsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.ElectricalPowerDensity, foUTElectricalPowerDensity);
            //UTPipingDensity
            var foUTPipingDensity = units.GetFormatOptions(SpecTypeId.PipingDensity);
            foUTPipingDensity.Accuracy = 0.0001;
            foUTPipingDensity.SetUnitTypeId(UnitTypeId.PoundsMassPerCubicFoot);
            units.SetFormatOptions(SpecTypeId.PipingDensity, foUTPipingDensity);
            //UTPipingFlow
            var foUTPipingFlow = units.GetFormatOptions(SpecTypeId.Flow);
            foUTPipingFlow.Accuracy = 1;
            foUTPipingFlow.SetUnitTypeId(UnitTypeId.UsGallonsPerMinute);
            units.SetFormatOptions(SpecTypeId.Flow, foUTPipingFlow);
            //UTPipingFriction
            var foUTPipingFriction = units.GetFormatOptions(SpecTypeId.PipingFriction);
            foUTPipingFriction.Accuracy = 0.01;
            foUTPipingFriction.SetUnitTypeId(UnitTypeId.FeetOfWater39_2DegreesFahrenheitPer100Feet);
            units.SetFormatOptions(SpecTypeId.PipingFriction, foUTPipingFriction);
            //UTPipingPressure
            var foUTPipingPressure = units.GetFormatOptions(SpecTypeId.PipingPressure);
            foUTPipingPressure.Accuracy = 0.01;
            foUTPipingPressure.SetUnitTypeId(UnitTypeId.PoundsForcePerSquareInch);
            units.SetFormatOptions(SpecTypeId.PipingPressure, foUTPipingPressure);


            //UTPipingTemperature
            var foUTPipingTemperature = units.GetFormatOptions(SpecTypeId.PipingTemperature);
            foUTPipingTemperature.Accuracy = 1;
            foUTPipingTemperature.SetUnitTypeId(UnitTypeId.Fahrenheit);
            units.SetFormatOptions(SpecTypeId.PipingTemperature, foUTPipingTemperature);
            //UTPipingVelocity
            var foUTPipingVelocity = units.GetFormatOptions(SpecTypeId.PipingVelocity);
            foUTPipingVelocity.Accuracy = 1;
            foUTPipingVelocity.SetUnitTypeId(UnitTypeId.FeetPerSecond);
            units.SetFormatOptions(SpecTypeId.PipingVelocity, foUTPipingVelocity);
            //UTPipingViscosity
            var foUTPipingViscosity = units.GetFormatOptions(SpecTypeId.PipingViscosity);
            foUTPipingViscosity.Accuracy = 0.01;
            foUTPipingViscosity.SetUnitTypeId(UnitTypeId.Centipoises);
            units.SetFormatOptions(SpecTypeId.PipingViscosity, foUTPipingViscosity);
            //UTPipeSize
            var foUTPipeSize = units.GetFormatOptions(SpecTypeId.PipeSize);
            foUTPipeSize.Accuracy = 1;
            foUTPipeSize.SetUnitTypeId(UnitTypeId.FractionalInches);
            units.SetFormatOptions(SpecTypeId.PipeSize, foUTPipeSize);
            //UTPipingRoughness
            var foUTPipingRoughness = units.GetFormatOptions(SpecTypeId.PipingRoughness);
            foUTPipingRoughness.Accuracy = 1E-05;
            foUTPipingRoughness.SetUnitTypeId(UnitTypeId.Feet);
            units.SetFormatOptions(SpecTypeId.PipingRoughness, foUTPipingRoughness);
            //UTPipingVolume
            var foUTPipingVolume = units.GetFormatOptions(SpecTypeId.PipingVolume);
            foUTPipingVolume.Accuracy = 0.1;
            foUTPipingVolume.SetUnitTypeId(UnitTypeId.UsGallons);
            units.SetFormatOptions(SpecTypeId.PipingVolume, foUTPipingVolume);
            //UTHVACViscosity
            var foUTHVACViscosity = units.GetFormatOptions(SpecTypeId.HvacViscosity);
            foUTHVACViscosity.Accuracy = 0.01;
            foUTHVACViscosity.SetUnitTypeId(UnitTypeId.Centipoises);
            units.SetFormatOptions(SpecTypeId.HvacViscosity, foUTHVACViscosity);


            //UTHVACCoefficientOfHeatTransfer
            var foUTHVACCoefficientOfHeatTransfer = units.GetFormatOptions(SpecTypeId.HeatTransferCoefficient);
            foUTHVACCoefficientOfHeatTransfer.Accuracy = 0.0001;
            foUTHVACCoefficientOfHeatTransfer.SetUnitTypeId(UnitTypeId
                .BritishThermalUnitsPerHourSquareFootDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.HeatTransferCoefficient, foUTHVACCoefficientOfHeatTransfer);

            //UTHVACThermalResistance
            var foUTHVACThermalResistance = units.GetFormatOptions(SpecTypeId.ThermalResistance);
            foUTHVACThermalResistance.Accuracy = 0.0001;
            foUTHVACThermalResistance.SetUnitTypeId(UnitTypeId.HourSquareFootDegreesFahrenheitPerBritishThermalUnit);
            units.SetFormatOptions(SpecTypeId.ThermalResistance, foUTHVACThermalResistance);

            //UTHVACThermalMass
            var foUTHVACThermalMass = units.GetFormatOptions(SpecTypeId.ThermalMass);
            foUTHVACThermalMass.Accuracy = 0.0001;
            foUTHVACThermalMass.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.ThermalMass, foUTHVACThermalMass);
            //UTHVACThermalConductivity
            var foUTHVACThermalConductivity = units.GetFormatOptions(SpecTypeId.ThermalConductivity);
            foUTHVACThermalConductivity.Accuracy = 0.0001;
            foUTHVACThermalConductivity.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourFootDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.ThermalConductivity, foUTHVACThermalConductivity);

            //UTHVACSpecificHeat
            var foUTHVACSpecificHeat = units.GetFormatOptions(SpecTypeId.SpecificHeat);
            foUTHVACSpecificHeat.Accuracy = 0.0001;
            foUTHVACSpecificHeat.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerPoundDegreeFahrenheit);
            units.SetFormatOptions(SpecTypeId.SpecificHeat, foUTHVACSpecificHeat);
            //UTHVACSpecificHeatOfVaporization
            var foUTHVACSpecificHeatOfVaporization =
                units.GetFormatOptions(SpecTypeId.SpecificHeatOfVaporization);
            foUTHVACSpecificHeatOfVaporization.Accuracy = 0.0001;
            foUTHVACSpecificHeatOfVaporization.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerPound);
            units.SetFormatOptions(SpecTypeId.SpecificHeatOfVaporization, foUTHVACSpecificHeatOfVaporization);

            //UTHVACPermeability
            var foUTHVACPermeability = units.GetFormatOptions(SpecTypeId.Permeability);
            foUTHVACPermeability.Accuracy = 0.0001;
            foUTHVACPermeability.SetUnitTypeId(UnitTypeId.GrainsPerHourSquareFootInchMercury);
            units.SetFormatOptions(SpecTypeId.Permeability, foUTHVACPermeability);
            //UTElectricalResistivity
            var foUTElectricalResistivity = units.GetFormatOptions(SpecTypeId.ElectricalResistivity);
            foUTElectricalResistivity.Accuracy = 0.0001;
            foUTElectricalResistivity.SetUnitTypeId(UnitTypeId.OhmMeters);
            units.SetFormatOptions(SpecTypeId.ElectricalResistivity, foUTElectricalResistivity);

            //UTHVACAirFlowDensity
            var foUTHVACAirFlowDensity = units.GetFormatOptions(SpecTypeId.AirFlowDensity);
            foUTHVACAirFlowDensity.Accuracy = 0.01;
            foUTHVACAirFlowDensity.SetUnitTypeId(UnitTypeId.CubicFeetPerMinuteSquareFoot);
            units.SetFormatOptions(SpecTypeId.AirFlowDensity, foUTHVACAirFlowDensity);
            //UTSlope
            var foUTSlope = units.GetFormatOptions(SpecTypeId.Slope);
            foUTSlope.Accuracy = 0.01;
            foUTSlope.SetUnitTypeId(UnitTypeId.SlopeDegrees);
            units.SetFormatOptions(SpecTypeId.Slope, foUTSlope);


            //UTHVACCoolingLoad
            var foUTHVACCoolingLoad = units.GetFormatOptions(SpecTypeId.CoolingLoad);
            foUTHVACCoolingLoad.Accuracy = 0.1;
            foUTHVACCoolingLoad.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.CoolingLoad, foUTHVACCoolingLoad);
            //UTHVACHeatingLoad
            var foUTHVACHeatingLoad = units.GetFormatOptions(SpecTypeId.HeatingLoad);
            foUTHVACHeatingLoad.Accuracy = 0.1;
            foUTHVACHeatingLoad.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.HeatingLoad, foUTHVACHeatingLoad);
            //UTHVACCoolingLoadDividedByArea
            var foUTHVACCoolingLoadDividedByArea = units.GetFormatOptions(SpecTypeId.CoolingLoadDividedByArea);
            foUTHVACCoolingLoadDividedByArea.Accuracy = 0.01;
            foUTHVACCoolingLoadDividedByArea.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourSquareFoot);
            units.SetFormatOptions(SpecTypeId.CoolingLoadDividedByArea, foUTHVACCoolingLoadDividedByArea);
            //UTHVACHeatingLoadDividedByArea
            var foUTHVACHeatingLoadDividedByArea =
                units.GetFormatOptions(SpecTypeId.HeatingLoadDividedByArea);
            foUTHVACHeatingLoadDividedByArea.Accuracy = 0.01;
            foUTHVACHeatingLoadDividedByArea.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourSquareFoot);
            units.SetFormatOptions(SpecTypeId.HeatingLoadDividedByArea, foUTHVACHeatingLoadDividedByArea);
            //UTHVACCoolingLoadDividedByVolume
            var foUTHVACCoolingLoadDividedByVolume =
                units.GetFormatOptions(SpecTypeId.CoolingLoadDividedByVolume);
            foUTHVACCoolingLoadDividedByVolume.Accuracy = 0.01;
            foUTHVACCoolingLoadDividedByVolume.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourCubicFoot);
            units.SetFormatOptions(SpecTypeId.CoolingLoadDividedByVolume, foUTHVACCoolingLoadDividedByVolume);
            //UTHVACHeatingLoadDividedByVolume
            var foUTHVACHeatingLoadDividedByVolume =
                units.GetFormatOptions(SpecTypeId.HeatingLoadDividedByVolume);
            foUTHVACHeatingLoadDividedByVolume.Accuracy = 0.01;
            foUTHVACHeatingLoadDividedByVolume.SetUnitTypeId(UnitTypeId.BritishThermalUnitsPerHourCubicFoot);
            units.SetFormatOptions(SpecTypeId.HeatingLoadDividedByVolume, foUTHVACHeatingLoadDividedByVolume);


            //UTHVACAirFlowDividedByVolume
            var foUTHVACAirFlowDividedByVolume = units.GetFormatOptions(SpecTypeId.AirFlowDividedByVolume);
            foUTHVACAirFlowDividedByVolume.Accuracy = 0.01;
            foUTHVACAirFlowDividedByVolume.SetUnitTypeId(UnitTypeId.CubicFeetPerMinuteCubicFoot);
            units.SetFormatOptions(SpecTypeId.AirFlowDividedByVolume, foUTHVACAirFlowDividedByVolume);
            //UTHVACAirFlowDividedByCoolingLoad
            var foUTHVACAirFlowDividedByCoolingLoad =
                units.GetFormatOptions(SpecTypeId.AirFlowDividedByCoolingLoad);
            foUTHVACAirFlowDividedByCoolingLoad.Accuracy = 0.01;
            foUTHVACAirFlowDividedByCoolingLoad.SetUnitTypeId(UnitTypeId.CubicFeetPerMinuteTonOfRefrigeration);
            units.SetFormatOptions(SpecTypeId.AirFlowDividedByCoolingLoad,
                foUTHVACAirFlowDividedByCoolingLoad);
            //UTHVACAreaDividedByCoolingLoad
            var foUTHVACAreaDividedByCoolingLoad =
                units.GetFormatOptions(SpecTypeId.AreaDividedByCoolingLoad);
            foUTHVACAreaDividedByCoolingLoad.Accuracy = 0.01;
            foUTHVACAreaDividedByCoolingLoad.SetUnitTypeId(UnitTypeId.SquareFeetPerTonOfRefrigeration);
            units.SetFormatOptions(SpecTypeId.AreaDividedByCoolingLoad, foUTHVACAreaDividedByCoolingLoad);
            //UTHVACAreaDividedByHeatingLoad
            var foUTHVACAreaDividedByHeatingLoad =
                units.GetFormatOptions(SpecTypeId.AreaDividedByHeatingLoad);
            foUTHVACAreaDividedByHeatingLoad.Accuracy = 0.0001;
            foUTHVACAreaDividedByHeatingLoad.SetUnitTypeId(UnitTypeId.SquareFeetPer1000BritishThermalUnitsPerHour);
            units.SetFormatOptions(SpecTypeId.AreaDividedByHeatingLoad, foUTHVACAreaDividedByHeatingLoad);


            //UTWireSize
            var foUTWireSize = units.GetFormatOptions(SpecTypeId.WireDiameter);
            foUTWireSize.Accuracy = 1E-06;
            foUTWireSize.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.WireDiameter, foUTWireSize);
            //UTHVACSlope
            var foUTHVACSlope = units.GetFormatOptions(SpecTypeId.HvacSlope);
            foUTHVACSlope.Accuracy = 0.03125;
            foUTHVACSlope.SetUnitTypeId(UnitTypeId.RiseDividedBy120Inches);
            units.SetFormatOptions(SpecTypeId.HvacSlope, foUTHVACSlope);
            //UTPipingSlope
            var foUTPipingSlope = units.GetFormatOptions(SpecTypeId.PipingSlope);
            foUTPipingSlope.Accuracy = 0.03125;
            foUTPipingSlope.SetUnitTypeId(UnitTypeId.RiseDividedBy120Inches);
            units.SetFormatOptions(SpecTypeId.PipingSlope, foUTPipingSlope);
            //UTCurrency
            var foUTCurrency = units.GetFormatOptions(SpecTypeId.Currency);
            foUTCurrency.Accuracy = 0.01;
            foUTCurrency.SetUnitTypeId(UnitTypeId.Currency);
            units.SetFormatOptions(SpecTypeId.Currency, foUTCurrency);
            //UTMassDensity
            var foUTMassDensity = units.GetFormatOptions(SpecTypeId.MassDensity);
            foUTMassDensity.Accuracy = 0.01;
            foUTMassDensity.SetUnitTypeId(UnitTypeId.PoundsMassPerCubicFoot);
            units.SetFormatOptions(SpecTypeId.MassDensity, foUTMassDensity);
            //UTHVACFactor
            var foUTHVACFactor = units.GetFormatOptions(SpecTypeId.Factor);
            foUTHVACFactor.Accuracy = 0.01;
            foUTHVACFactor.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.Factor, foUTHVACFactor);

            //UTElectricalTemperature
            var foUTElectricalTemperature = units.GetFormatOptions(SpecTypeId.ElectricalTemperature);
            foUTElectricalTemperature.Accuracy = 1;
            foUTElectricalTemperature.SetUnitTypeId(UnitTypeId.Fahrenheit);
            units.SetFormatOptions(SpecTypeId.ElectricalTemperature, foUTElectricalTemperature);
            //UTElectricalCableTraySize
            var foUTElectricalCableTraySize = units.GetFormatOptions(SpecTypeId.CableTraySize);
            foUTElectricalCableTraySize.Accuracy = 0.25;
            foUTElectricalCableTraySize.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.CableTraySize, foUTElectricalCableTraySize);
            //UTElectricalConduitSize
            var foUTElectricalConduitSize = units.GetFormatOptions(SpecTypeId.ConduitSize);
            foUTElectricalConduitSize.Accuracy = 0.125;
            foUTElectricalConduitSize.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.ConduitSize, foUTElectricalConduitSize);
            //UTElectricalDemandFactor
            var foUTElectricalDemandFactor = units.GetFormatOptions(SpecTypeId.DemandFactor);
            foUTElectricalDemandFactor.Accuracy = 0.01;
            foUTElectricalDemandFactor.SetUnitTypeId(UnitTypeId.Percentage);
            units.SetFormatOptions(SpecTypeId.DemandFactor, foUTElectricalDemandFactor);
            //UTHVACDuctInsulationThickness
            var foUTHVACDuctInsulationThickness = units.GetFormatOptions(SpecTypeId.DuctInsulationThickness);
            foUTHVACDuctInsulationThickness.Accuracy = 1;
            foUTHVACDuctInsulationThickness.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.DuctInsulationThickness, foUTHVACDuctInsulationThickness);
            //UTHVACDuctLiningThickness
            var foUTHVACDuctLiningThickness = units.GetFormatOptions(SpecTypeId.DuctLiningThickness);
            foUTHVACDuctLiningThickness.Accuracy = 1;
            foUTHVACDuctLiningThickness.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.DuctLiningThickness, foUTHVACDuctLiningThickness);
            //UTPipeInsulationThickness
            var foUTPipeInsulationThickness = units.GetFormatOptions(SpecTypeId.PipeInsulationThickness);
            foUTPipeInsulationThickness.Accuracy = 1;
            foUTPipeInsulationThickness.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.PipeInsulationThickness, foUTPipeInsulationThickness);


            //UTForce
            var foUTForce = units.GetFormatOptions(SpecTypeId.Force);
            foUTForce.Accuracy = 0.01;
            foUTForce.SetUnitTypeId(UnitTypeId.Kips);
            units.SetFormatOptions(SpecTypeId.Force, foUTForce);
            //UTLinearForce
            var foUTLinearForce = units.GetFormatOptions(SpecTypeId.LinearForce);
            foUTLinearForce.Accuracy = 0.001;
            foUTLinearForce.SetUnitTypeId(UnitTypeId.KipsPerFoot);
            units.SetFormatOptions(SpecTypeId.LinearForce, foUTLinearForce);
            //UTAreaForce
            var foUTAreaForce = units.GetFormatOptions(SpecTypeId.AreaForce);
            foUTAreaForce.Accuracy = 0.0001;
            foUTAreaForce.SetUnitTypeId(UnitTypeId.KipsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.AreaForce, foUTAreaForce);
            //UTMoment
            var foUTMoment = units.GetFormatOptions(SpecTypeId.Moment);
            foUTMoment.Accuracy = 0.01;
            foUTMoment.SetUnitTypeId(UnitTypeId.KipFeet);
            units.SetFormatOptions(SpecTypeId.Moment, foUTMoment);
            //UTLinearMoment
            var foUTLinearMoment = units.GetFormatOptions(SpecTypeId.LinearMoment);
            foUTLinearMoment.Accuracy = 0.01;
            foUTLinearMoment.SetUnitTypeId(UnitTypeId.KipFeetPerFoot);
            units.SetFormatOptions(SpecTypeId.LinearMoment, foUTLinearMoment);
            //UTStress
            var foUTStress = units.GetFormatOptions(SpecTypeId.Stress);
            foUTStress.Accuracy = 0.01;
            foUTStress.SetUnitTypeId(UnitTypeId.KipsPerSquareInch);
            units.SetFormatOptions(SpecTypeId.Stress, foUTStress);
            //UTUnitWeight
            var foUTUnitWeight = units.GetFormatOptions(SpecTypeId.UnitWeight);
            foUTUnitWeight.Accuracy = 0.01;
            foUTUnitWeight.SetUnitTypeId(UnitTypeId.PoundsForcePerCubicFoot);
            units.SetFormatOptions(SpecTypeId.UnitWeight, foUTUnitWeight);
            //UTWeight
            var foUTWeight = units.GetFormatOptions(SpecTypeId.Weight);
            foUTWeight.Accuracy = 0.01;
            foUTWeight.SetUnitTypeId(UnitTypeId.PoundsForce);
            units.SetFormatOptions(SpecTypeId.Weight, foUTWeight);
            //UTMass
            var foUTMass = units.GetFormatOptions(SpecTypeId.Mass);
            foUTMass.Accuracy = 0.01;
            foUTMass.SetUnitTypeId(UnitTypeId.PoundsMass);
            units.SetFormatOptions(SpecTypeId.Mass, foUTMass);


            //UTMassPerUnitArea
            var foUTMassPerUnitArea = units.GetFormatOptions(SpecTypeId.MassPerUnitArea);
            foUTMassPerUnitArea.Accuracy = 0.01;
            foUTMassPerUnitArea.SetUnitTypeId(UnitTypeId.PoundsMassPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.MassPerUnitArea, foUTMassPerUnitArea);


            //UTThermalExpansion
            var foUTThermalExpansion = units.GetFormatOptions(SpecTypeId.ThermalExpansionCoefficient);
            foUTThermalExpansion.Accuracy = 1E-05;
            foUTThermalExpansion.SetUnitTypeId(UnitTypeId.InverseDegreesFahrenheit);
            units.SetFormatOptions(SpecTypeId.ThermalExpansionCoefficient, foUTThermalExpansion);

            //UTForcePerLength
            var foUTForcePerLength = units.GetFormatOptions(SpecTypeId.LinearForce);
            foUTForcePerLength.Accuracy = 0.1;
            foUTForcePerLength.SetUnitTypeId(UnitTypeId.KipsPerInch);
            units.SetFormatOptions(SpecTypeId.LinearForce, foUTForcePerLength);
            //UTLinearForcePerLength


            //UTAreaForcePerLength
            var foUTAreaForcePerLength = units.GetFormatOptions(SpecTypeId.AreaForce);
            foUTAreaForcePerLength.Accuracy = 0.1;
            foUTAreaForcePerLength.SetUnitTypeId(UnitTypeId.KipsPerSquareFoot);
            units.SetFormatOptions(SpecTypeId.AreaForce, foUTAreaForcePerLength);


            //UTDisplacementDeflection
            var foUTDisplacementDeflection = units.GetFormatOptions(SpecTypeId.Displacement);
            foUTDisplacementDeflection.Accuracy = 0.01;
            foUTDisplacementDeflection.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.Displacement, foUTDisplacementDeflection);


            //UTRotation
            var foUTRotation = units.GetFormatOptions(SpecTypeId.Rotation);
            foUTRotation.Accuracy = 0.001;
            foUTRotation.SetUnitTypeId(UnitTypeId.Degrees);
            units.SetFormatOptions(SpecTypeId.Rotation, foUTRotation);
            //UTPeriod
            var foUTPeriod = units.GetFormatOptions(SpecTypeId.Period);
            foUTPeriod.Accuracy = 0.1;
            foUTPeriod.SetUnitTypeId(UnitTypeId.Seconds);
            units.SetFormatOptions(SpecTypeId.Period, foUTPeriod);
            //UTStructuralFrequency
            var foUTStructuralFrequency = units.GetFormatOptions(SpecTypeId.StructuralFrequency);
            foUTStructuralFrequency.Accuracy = 0.1;
            foUTStructuralFrequency.SetUnitTypeId(UnitTypeId.Hertz);
            units.SetFormatOptions(SpecTypeId.StructuralFrequency, foUTStructuralFrequency);
            //UTPulsation
            var foUTPulsation = units.GetFormatOptions(SpecTypeId.Pulsation);
            foUTPulsation.Accuracy = 0.1;
            foUTPulsation.SetUnitTypeId(UnitTypeId.RadiansPerSecond);
            units.SetFormatOptions(SpecTypeId.Pulsation, foUTPulsation);
            //UTStructuralVelocity
            var foUTStructuralVelocity = units.GetFormatOptions(SpecTypeId.StructuralVelocity);
            foUTStructuralVelocity.Accuracy = 0.1;
            foUTStructuralVelocity.SetUnitTypeId(UnitTypeId.FeetPerSecond);
            units.SetFormatOptions(SpecTypeId.StructuralVelocity, foUTStructuralVelocity);


            //UTAcceleration
            var foUTAcceleration = units.GetFormatOptions(SpecTypeId.Acceleration);
            foUTAcceleration.Accuracy = 0.1;
            foUTAcceleration.SetUnitTypeId(UnitTypeId.FeetPerSecondSquared);
            units.SetFormatOptions(SpecTypeId.Acceleration, foUTAcceleration);
            //UTEnergy
            var foUTEnergy = units.GetFormatOptions(SpecTypeId.Energy);
            foUTEnergy.Accuracy = 0.1;
            foUTEnergy.SetUnitTypeId(UnitTypeId.PoundForceFeet);
            units.SetFormatOptions(SpecTypeId.Energy, foUTEnergy);
            //UTReinforcementVolume
            var foUTReinforcementVolume = units.GetFormatOptions(SpecTypeId.ReinforcementVolume);
            foUTReinforcementVolume.Accuracy = 0.01;
            foUTReinforcementVolume.SetUnitTypeId(UnitTypeId.CubicInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementVolume, foUTReinforcementVolume);
            //UTReinforcementLength
            var foUTReinforcementLength = units.GetFormatOptions(SpecTypeId.ReinforcementLength);
            foUTReinforcementLength.Accuracy = 0.00260416666666667;
            foUTReinforcementLength.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementLength, foUTReinforcementLength);

            //UTReinforcementArea
            var foUTReinforcementArea = units.GetFormatOptions(SpecTypeId.ReinforcementArea);
            foUTReinforcementArea.Accuracy = 0.01;
            foUTReinforcementArea.SetUnitTypeId(UnitTypeId.SquareInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementArea, foUTReinforcementArea);
            //UTReinforcementAreaperUnitLength
            var foUTReinforcementAreaperUnitLength =
                units.GetFormatOptions(SpecTypeId.ReinforcementAreaPerUnitLength);
            foUTReinforcementAreaperUnitLength.Accuracy = 0.01;
            foUTReinforcementAreaperUnitLength.SetUnitTypeId(UnitTypeId.SquareInchesPerFoot);
            units.SetFormatOptions(SpecTypeId.ReinforcementAreaPerUnitLength, foUTReinforcementAreaperUnitLength);
            //UTReinforcementSpacing
            var foUTReinforcementSpacing = units.GetFormatOptions(SpecTypeId.ReinforcementSpacing);
            foUTReinforcementSpacing.Accuracy = 0.00260416666666667;
            foUTReinforcementSpacing.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementSpacing, foUTReinforcementSpacing);
            //UTReinforcementCover
            var foUTReinforcementCover = units.GetFormatOptions(SpecTypeId.ReinforcementCover);
            foUTReinforcementCover.Accuracy = 0.00260416666666667;
            foUTReinforcementCover.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.ReinforcementCover, foUTReinforcementCover);

            //UTBarDiameter
            var foUTBarDiameter = units.GetFormatOptions(SpecTypeId.BarDiameter);
            foUTBarDiameter.Accuracy = 0.00260416666666667;
            foUTBarDiameter.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.BarDiameter, foUTBarDiameter);
            //UTCrackWidth
            var foUTCrackWidth = units.GetFormatOptions(SpecTypeId.CrackWidth);
            foUTCrackWidth.Accuracy = 0.01;
            foUTCrackWidth.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.CrackWidth, foUTCrackWidth);
            //UTSectionDimension
            var foUTSectionDimension = units.GetFormatOptions(SpecTypeId.SectionDimension);
            foUTSectionDimension.Accuracy = 0.00520833333333333;
            foUTSectionDimension.SetUnitTypeId(UnitTypeId.FeetFractionalInches);
            units.SetFormatOptions(SpecTypeId.SectionDimension, foUTSectionDimension);
            //UTSectionProperty
            var foUTSectionProperty = units.GetFormatOptions(SpecTypeId.SectionProperty);
            foUTSectionProperty.Accuracy = 0.001;
            foUTSectionProperty.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.SectionProperty, foUTSectionProperty);
            //UTSectionArea
            var foUTSectionArea = units.GetFormatOptions(SpecTypeId.SectionArea);
            foUTSectionArea.Accuracy = 0.01;
            foUTSectionArea.SetUnitTypeId(UnitTypeId.SquareInches);
            units.SetFormatOptions(SpecTypeId.SectionArea, foUTSectionArea);

            //UTSectionModulus
            var foUTSectionModulus = units.GetFormatOptions(SpecTypeId.SectionModulus);
            foUTSectionModulus.Accuracy = 0.01;
            foUTSectionModulus.SetUnitTypeId(UnitTypeId.CubicInches);
            units.SetFormatOptions(SpecTypeId.SectionModulus, foUTSectionModulus);
            //UTMomentofInertia
            var foUTMomentofInertia = units.GetFormatOptions(SpecTypeId.MomentOfInertia);
            foUTMomentofInertia.Accuracy = 0.01;
            foUTMomentofInertia.SetUnitTypeId(UnitTypeId.InchesToTheFourthPower);
            units.SetFormatOptions(SpecTypeId.MomentOfInertia, foUTMomentofInertia);
            //UTWarpingConstant
            var foUTWarpingConstant = units.GetFormatOptions(SpecTypeId.WarpingConstant);
            foUTWarpingConstant.Accuracy = 0.01;
            foUTWarpingConstant.SetUnitTypeId(UnitTypeId.InchesToTheSixthPower);
            units.SetFormatOptions(SpecTypeId.WarpingConstant, foUTWarpingConstant);
            //UTMassperUnitLength
            var foUTMassperUnitLength = units.GetFormatOptions(SpecTypeId.MassPerUnitLength);
            foUTMassperUnitLength.Accuracy = 0.01;
            foUTMassperUnitLength.SetUnitTypeId(UnitTypeId.PoundsMassPerFoot);
            units.SetFormatOptions(SpecTypeId.MassPerUnitLength, foUTMassperUnitLength);
            //UTWeightperUnitLength
            var foUTWeightperUnitLength = units.GetFormatOptions(SpecTypeId.WeightPerUnitLength);
            foUTWeightperUnitLength.Accuracy = 0.01;
            foUTWeightperUnitLength.SetUnitTypeId(UnitTypeId.PoundsForcePerFoot);
            units.SetFormatOptions(SpecTypeId.WeightPerUnitLength, foUTWeightperUnitLength);

            //UTSurfaceArea
            var foUTSurfaceArea = units.GetFormatOptions(SpecTypeId.SurfaceAreaPerUnitLength);
            foUTSurfaceArea.Accuracy = 0.01;
            foUTSurfaceArea.SetUnitTypeId(UnitTypeId.SquareFeetPerFoot);
            units.SetFormatOptions(SpecTypeId.SurfaceAreaPerUnitLength, foUTSurfaceArea);
            //UTPipeDimension
            var foUTPipeDimension = units.GetFormatOptions(SpecTypeId.PipeDimension);
            foUTPipeDimension.Accuracy = 0.001;
            foUTPipeDimension.SetUnitTypeId(UnitTypeId.Inches);
            units.SetFormatOptions(SpecTypeId.PipeDimension, foUTPipeDimension);

            //UTPipeMass
            var foUTPipeMass = units.GetFormatOptions(SpecTypeId.PipeMassPerUnitLength);
            foUTPipeMass.Accuracy = 0.01;
            foUTPipeMass.SetUnitTypeId(UnitTypeId.PoundsMassPerFoot);
            units.SetFormatOptions(SpecTypeId.PipeMassPerUnitLength, foUTPipeMass);

            //UTPipeMassPerUnitLength
            var foUTPipeMassPerUnitLength = units.GetFormatOptions(SpecTypeId.PipeMassPerUnitLength);
            foUTPipeMassPerUnitLength.Accuracy = 0.01;
            foUTPipeMassPerUnitLength.SetUnitTypeId(UnitTypeId.PoundsMassPerFoot);
            units.SetFormatOptions(SpecTypeId.PipeMassPerUnitLength, foUTPipeMassPerUnitLength);


            using (var t = new Transaction(doc, "Convert to Imperial"))
            {
                t.Start();

                doc.SetUnits(units);

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}